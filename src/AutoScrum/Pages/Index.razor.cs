using AntDesign;
using AutoScrum.AzureDevOps;
using AutoScrum.Core.Config;
using AutoScrum.Core.Models;
using AutoScrum.Core.Services;
using AutoScrum.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OldConfigService = AutoScrum.Services.OldConfigService;

namespace AutoScrum.Pages
{
    public partial class Index
    {
        private const int ContentSpan = 21;
        private const int AnchorSpan = 3;

        private Form<ProjectConfigAzureDevOps> _connectionForm;
        private bool _connectionFormLoading;

        private bool IsPageInitializing { get; set; } = true;

        private List<WorkItem>? _cachedWorkItems;

        [Inject] public IDailyScrumService DailyScrum { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }
        // TODO: Remove when all is updated.
        [Inject] public OldConfigService OldConfigService { get; set; }
        [Inject] public IConfigService ConfigService { get; set; }
        [Inject] public MessageService MessageService { get; set; }
        [Inject] public IClipboardService ClipboardService { get; set; }

        public List<ProjectMetadata> ProjectMetadatas { get; set; } = new();

        private MarkupString Output { get; set; } = (MarkupString)"";

        private ProjectConfigAzureDevOps ConnectionInfo { get; set; } = new();
        private ProjectConfigAzureDevOps ConnectionInfoRequest { get; set; } = new();
        private List<User> Users { get; set; } = new();
        private List<User> IncludedUsers { get; set; } = new();

        private int? _selectedProjectId;
        private string _markdown = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                AppConfig config = await ConfigService.GetAppConfig();
                if (config != null)
                {
                    ProjectMetadatas = await ConfigService.GetProjectsMetadata();
                    await UpdateCurrentProjectUI(true);
                }
            }
            catch
            {
                IsPageInitializing = false;
                MessageService.Error("Critical error while loading config");
                throw;
            }

            if (ConnectionInfo is null)
            {
                IsPageInitializing = false;
                return;
            }

            IsPageInitializing = false;
        }

        private async Task UpdateCurrentProjectUI(bool autoUpdateDevOps)
        {
            ProjectConfig? project = await ConfigService.GetCurrentProject();
            if (project is ProjectConfigAzureDevOps azureDevOpsProject)
            {
                ConnectionInfo = azureDevOpsProject;
                ConnectionInfoRequest = ConnectionInfo.Clone();

                _selectedProjectId = project.Id;

                if (autoUpdateDevOps)
                {
                    try
                    {
                        // TODO: This should be postponed so that theme and UI can be updated.
                        await GetDataFromAzureDevOpsAsync();
                    }
                    catch
                    {
                        // TODO: Log?
                    }
                }
            }

            ProjectMetadatas = await ConfigService.GetProjectsMetadata();
            StateHasChanged();
        }

        private async Task SubmitAsync()
        {
            _connectionFormLoading = true;

            if (_connectionForm.Validate())
            {
                ReloadConnectionInfoFromForm();
                await GetDataFromAzureDevOpsAsync();
                MessageService.Success("Loaded data from Azure DevOps!");
            }

            _connectionFormLoading = false;
        }

        private async Task SaveConfigAsync()
        {
            _connectionFormLoading = true;

            if (_connectionForm.Validate())
            {
                ReloadConnectionInfoFromForm();

                ConnectionInfo = await ConfigService.AddOrUpdateProject(ConnectionInfo);
                ConnectionInfoRequest = ConnectionInfo.Clone();

                // Atm, only current project is supported.
                await ConfigService.SetCurrentProject(ConnectionInfo.Id);

                await OldConfigService.SetConfig(ConnectionInfo);

                MessageService.Success("Config saved successfully!");

                await UpdateCurrentProjectUI(false);
            }

            _connectionFormLoading = false;
        }

        private async Task DeleteConfigAsync()
        {
            _connectionFormLoading = true;

            await ConfigService.RemoveProject(ConnectionInfo);

            ConnectionInfo = null;
            ConnectionInfoRequest = new ProjectConfigAzureDevOps();
            MessageService.Success("Config deleted successfully!");

            _connectionFormLoading = false;

            await UpdateCurrentProjectUI(true);

            StateHasChanged();
        }

        private AzureDevOpsService GetAzureDevOpsService() => new(ConnectionInfo, HttpClient);

        private async Task<Sprint?> GetCurrentSprint(AzureDevOpsService? azureDevOpsService = null)
        {
            azureDevOpsService ??= GetAzureDevOpsService();

            var currentSprint = await azureDevOpsService.GetCurrentSprint();
            Console.WriteLine("Current Sprint: " + (currentSprint?.Name ?? "Null"));

            return currentSprint;
        }

        private async Task GetDataFromAzureDevOpsAsync()
        {
            if (!EnsureConnectionInfoValid())
            {
                return;
            }

            try
            {
                var sprint = await GetCurrentSprint();
                if (sprint?.Path is null)
                {
                    MessageService.Warning("No current sprint found");
                    return;
                }

                _cachedWorkItems = await GetAzureDevOpsService()
                    .GetWorkItemsForSprint(sprint, ConnectionInfo!.TeamFilterBy);

                Users = GetUniqueUsers(_cachedWorkItems);
                ReloadUsers();

                ReloadWorkItems();

                StateHasChanged();
            }
            catch
            {
                MessageService.Error("Critical error while loading data from Azure DevOps");
            }
        }

        private void ReloadConnectionInfoFromForm()
        {
            ConnectionInfo = ConnectionInfoRequest.Clone();
        }

        private void ReloadUsers()
        {
            IncludedUsers = Users.Where(x => x.Included).ToList();
        }

        private void ReloadWorkItems()
        {
            foreach (var item in _cachedWorkItems!)
            {
                Console.WriteLine($"{item.Type} {item.Id}: {item.Title} - {item.AssignedToEmail}");
            }

            DailyScrum.SetWorkItems(_cachedWorkItems, Users);

            UpdateOutput();
        }

        private void UpdateOutput()
        {
            _markdown = DailyScrum.GenerateReport(Users.Where(x => x.Included).ToList(), ProjectType.AzureDevOps, ReportOutputType.Markdown);
            var html = !string.IsNullOrWhiteSpace(_markdown)
                ? Markdig.Markdown.ToHtml(_markdown)
                : string.Empty;
            Output = (MarkupString)html;

            StateHasChanged();
        }

        private void AddYesterday(WorkItem wi)
        {
            DailyScrum.AddYesterday(wi);
            UpdateOutput();
        }

        private void AddToday(WorkItem wi)
        {
            DailyScrum.AddToday(wi);
            UpdateOutput();
        }

        private void RemoveWorkItem(WorkItem wi, bool isToday)
        {
            if (isToday)
            {
                DailyScrum.RemoveToday(wi);
            }
            else
            {
                DailyScrum.RemoveYesterday(wi);
            }

            UpdateOutput();
        }

        private static List<User> GetUniqueUsers(IReadOnlyCollection<WorkItem> workItems)
        {
            Dictionary<string, User> users = new();

            if (!workItems.Any())
            {
                return users.Values.ToList();
            }

            // TODO: If a user is only in a task but no PBI, this won't include them.
            foreach (var wi in workItems.Where(x => !string.IsNullOrWhiteSpace(x.AssignedToEmail) && !string.IsNullOrWhiteSpace(x.AssignedToDisplayName)))
            {
                if (!users.ContainsKey(wi.AssignedToEmail!))
                {
                    users.Add(wi.AssignedToEmail!, new User(wi.AssignedToDisplayName!, wi.AssignedToEmail!));
                }
            }

            return users.Values.ToList();
        }

        private void UserIncludeChanged(User user, bool isIncluded)
        {
            user.Included = isIncluded;

            ReloadUsers();
            ReloadWorkItems();
        }

        private bool EnsureConnectionInfoValid()
        {
            try
            {
                if (ConnectionInfo is null)
                {
                    throw new ArgumentNullException(nameof(ConnectionInfo));
                }

                ConnectionInfo.EnsureValid();
            }
            catch
            {
                MessageService.Error("Validate connection config");
                return false;
            }

            return true;
        }

        private string GetGenerateForLabel() => "Generate for " + ConnectionInfoRequest.TeamFilterBy;
        private void OnBlockingUpdated() => UpdateOutput();

        private async Task AddProject()
        {
            var project = await ConfigService.AddOrUpdateProject(new ProjectConfigAzureDevOps
            {
                Name = "New Project",
                ProjectType = ProjectType.AzureDevOps
            });

            await ConfigService.SetCurrentProject(project.Id);
            await UpdateCurrentProjectUI(false);
        }

        private async void OnSelectedItemChangedHandler(ProjectMetadata value)
        {
            if (value == null || value.Id == ConnectionInfo?.Id)
            {
                return;
            }

            await ConfigService.SetCurrentProject(value.Id);
            await UpdateCurrentProjectUI(true);

            Console.WriteLine($"selected: ${value?.Name}");
        }

        private void OnBlur()
        {
            Console.WriteLine("blur");
        }

        private void OnFocus()
        {
            Console.WriteLine("focus");
        }

        private void OnSearch(string value)
        {
            Console.WriteLine($"search: {value}");
        }

        private async Task CopyCommitMessage()
        {
            await ClipboardService.Copy(_markdown, Output.Value);
            MessageService.Success("Daily Scrum copied to clipboard!");
        }
    }
}
