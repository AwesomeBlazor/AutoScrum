using AntDesign;
using AutoScrum.AzureDevOps;
using AutoScrum.Core.Config;
using AutoScrum.Core.Models;
using AutoScrum.Core.Services;
using AutoScrum.Models;
using AutoScrum.Services;
using Microsoft.AspNetCore.Components;
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
        private readonly DailyScrumService _dailyScrum = new();

        private const int ContentSpan = 21;
        private const int AnchorSpan = 3;

        private Form<ProjectConfigAzureDevOps> _connectionForm;
        private bool _connectionFormLoading;

        private bool IsPageInitializing { get; set; } = true;

        private List<WorkItem>? _cachedWorkItems;

        [Inject] public HttpClient HttpClient { get; set; }
        // TODO: Remove when all is updated.
        [Inject] public OldConfigService OldConfigService { get; set; }
        [Inject] public IConfigService ConfigService { get; set; }
        [Inject] public MessageService MessageService { get; set; }

        private MarkupString Output { get; set; } = (MarkupString)"";

        private ProjectConfigAzureDevOps ConnectionInfo { get; set; } = new();
        private List<User> Users { get; set; } = new();
        private List<User> IncludedUsers { get; set; } = new();
        private User SelectedUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                AppConfig config = await ConfigService.GetAppConfig();
                if (config != null)
                {
                    var project = await ConfigService.GetCurrentProject();
                    if (project is ProjectConfigAzureDevOps azureDevOpsProject)
                    {
                        ConnectionInfo = azureDevOpsProject;

                        // TODO: This should be postponed so that theme and UI can be updated.
                        await GetDataFromAzureDevOpsAsync();
                    }
                }
            }
            catch
            {
                MessageService.Error("Critical error while loading config");
                throw;
            }

            await base.OnInitializedAsync();

            IsPageInitializing = false;
        }

        private async Task SubmitAsync()
        {
            _connectionFormLoading = true;
            
            if (_connectionForm.Validate())
            {
                await GetDataFromAzureDevOpsAsync();
                MessageService.Success("Loaded data from Azure DevOps!");
            }
            else
            {
                MessageService.Warning("Some validations failed...");
            }

            _connectionFormLoading = false;
        }

        private async Task SaveConfigAsync()
        {
            _connectionFormLoading = true;

            if (_connectionForm.Validate())
            {
                await OldConfigService.SetConfig(ConnectionInfo);
                MessageService.Success("Config saved successfully!");
            }
            
            _connectionFormLoading = false;
        }

        private async Task DeleteConfigAsync()
        {
            _connectionFormLoading = true;

            await OldConfigService.Clear();
            MessageService.Success("Config deleted successfully!");

            _connectionFormLoading = false;
        }
        
        private AzureDevOpsService GetAzureDevOpsService() => new(
            ConnectionInfo, HttpClient);

        private async Task<Sprint?> GetCurrentSprint(AzureDevOpsService azureDevOpsService = null)
        {
            azureDevOpsService ??= GetAzureDevOpsService();
            
            var currentSprint = await azureDevOpsService.GetCurrentSprint();
            Console.WriteLine("Current Sprint: " + currentSprint?.Name);

            return currentSprint;
        }

        private async Task<List<WorkItem>?> GetCurrentSprintWorkItems(AzureDevOpsService azureDevOpsService = null)
        {
            azureDevOpsService ??= GetAzureDevOpsService();

            var currentSprint = await GetCurrentSprint(azureDevOpsService);

            return await azureDevOpsService.GetWorkItemsForSprint(currentSprint, ConnectionInfo.TeamFilterBy);
        }

        private async Task GetDataFromAzureDevOpsAsync()
        {
            try
            {
                var sprint = await GetCurrentSprint();

                if (sprint == null)
                {
                    Console.WriteLine("Unable to load");
                }
                else
                {
                    _cachedWorkItems = await GetAzureDevOpsService()
                        .GetWorkItemsForSprint(sprint, ConnectionInfo.TeamFilterBy);

                    Users = GetUniqueUsers(_cachedWorkItems);
                    ReloadUsers();
                    
                    ReloadWorkItems();
                }

                StateHasChanged();
            }
            catch
            {
                MessageService.Error("Critical error while loading data from Azure DevOps");
                throw;
            }
            
        }

        private void ReloadUsers()
        {

            IncludedUsers = Users.Where(x => x.Included).ToList();
        }
        
        private void ReloadWorkItems()
        {
            foreach (var item in _cachedWorkItems)
            {
                Console.WriteLine($"{item.Type} {item.Id}: {item.Title}");
            }
            
            _dailyScrum.SetWorkItems(_cachedWorkItems, Users);
            SelectedUser = Users.FirstOrDefault();

            UpdateOutput();
        }

        private void UpdateOutput()
        {
            var markdown = _dailyScrum.GenerateReport(Users.Where(x => x.Included).ToList());
            var html = Markdig.Markdown.ToHtml(markdown ?? string.Empty);
            Output = (MarkupString)html;

            StateHasChanged();
        }

        private void AddYesterday(WorkItem wi)
        {
            _dailyScrum.AddYesterday(wi);
            UpdateOutput();
        }

        private void AddToday(WorkItem wi)
        {
            _dailyScrum.AddToday(wi);
            UpdateOutput();
        }

        private void RemoveWorkItem(WorkItem wi, bool isToday)
        {
            if (isToday)
            {
                _dailyScrum.RemoveToday(wi);
            }
            else
            {
                _dailyScrum.RemoveYesterday(wi);
            }

            UpdateOutput();
        }

        private List<User> GetUniqueUsers(List<WorkItem> workItems)
        {
            Dictionary<string, User> users = new();

            if (!workItems.Any())
            {
                return users.Values.ToList();
            }
            
            foreach (var wi in workItems.Where(x => !string.IsNullOrWhiteSpace(x.AssignedToEmail)))
            {
                if (!users.ContainsKey(wi.AssignedToEmail))
                {
                    users.Add(wi.AssignedToEmail, new User
                    {
                        DisplayName = wi.AssignedToDisplayName,
                        Email = wi.AssignedToEmail
                    });
                }
            }

            return users.Values.ToList();
        }

        private Task UserIncludeChangedAsync(User user, bool isIncluded)
        {
            user.Included = isIncluded;
            
            ReloadUsers();
            ReloadWorkItems();

            return Task.CompletedTask;
        }

        private void OnBlockingUpdated()
        {
            UpdateOutput();
        }

        private string GetGenerateForLabel() => "Generate for " + ConnectionInfo.TeamFilterBy;
    }
}
