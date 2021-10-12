using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AntDesign;
using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.AzureDevOps;
using AutoScrum.Services;
using Microsoft.AspNetCore.Components;
using AutoScrum.Models;
using ConfigService = AutoScrum.Services.ConfigService;

namespace AutoScrum.Pages
{
    public partial class Index
    {
        private readonly DailyScrumService _dailyScrum = new();

        private const int ContentSpan = 21;
        private const int AnchorSpan = 3;

        private Form<AzureDevOpsConnectionInfoRequest> _connectionForm = null!;
        private bool _connectionFormLoading;

        private bool IsPageInitializing { get; set; } = true;

        private List<WorkItem>? _cachedWorkItems;

        [Inject] public HttpClient HttpClient { get; set; } = null!;
        [Inject] public ConfigService ConfigService { get; set; } = null!;
        [Inject] public AutoMessageService MessageService { get; set; } = null!;

        private MarkupString Output { get; set; } = (MarkupString)"";

        private AzureDevOpsConnectionInfo? ConnectionInfo { get; set; }
        private AzureDevOpsConnectionInfoRequest ConnectionInfoRequest { get; set; } = new();
        private List<User> Users { get; set; } = new();
        private List<User> IncludedUsers { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var config = await ConfigService.GetConfig();
                ConnectionInfo = config;
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
            
            await GetDataFromAzureDevOpsAsync();
            
            IsPageInitializing = false;
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
                await ConfigService.SetConfig(ConnectionInfo!);
                MessageService.Success("Config saved successfully!");
            }
            
            _connectionFormLoading = false;
        }

        private async Task DeleteConfigAsync()
        {
            _connectionFormLoading = true;

            await ConfigService.Clear();
            
            ConnectionInfo = null;
            ConnectionInfoRequest = new AzureDevOpsConnectionInfoRequest();
            
            MessageService.Success("Config deleted successfully!");

            _connectionFormLoading = false;
        }
        
        private AzureDevOpsService GetAzureDevOpsService()
        {
            EnsureConnectionInfoValid();

            return new AzureDevOpsService(
                new AzureDevOpsConfig(ConnectionInfo!.AzureDevOpsOrganization!,
                    new Uri($"https://{ConnectionInfo.AzureDevOpsOrganization}.visualstudio.com"),
                    ConnectionInfo.ProjectName!, ConnectionInfo.UserEmail!, ConnectionInfo.PersonalAccessToken!),
                HttpClient);
        }

        private async Task<Sprint> GetCurrentSprint(AzureDevOpsService? azureDevOpsService = null)
        {
            azureDevOpsService ??= GetAzureDevOpsService();
            
            var currentSprint = await azureDevOpsService.GetCurrentSprint();
            Console.WriteLine("Current Sprint: " + currentSprint.Name);

            return currentSprint;
        }
        
        private async Task GetDataFromAzureDevOpsAsync()
        {
            try
            {
                var sprint = await GetCurrentSprint();

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
                throw;
            }
        }

        private void ReloadConnectionInfoFromForm()
        {
            ConnectionInfo = new AzureDevOpsConnectionInfo(ConnectionInfoRequest.UserEmail!, ConnectionInfoRequest.PersonalAccessToken!, ConnectionInfoRequest.AzureDevOpsOrganization!, ConnectionInfoRequest.ProjectName!, ConnectionInfoRequest.TeamFilterBy!);
        }

        private void ReloadUsers()
        {
            IncludedUsers = Users.Where(x => x.Included).ToList();
        }
        
        private void ReloadWorkItems()
        {
            foreach (var item in _cachedWorkItems!)
            {
                Console.WriteLine($"{item.Type} {item.Id}: {item.Title}");
            }
            
            _dailyScrum.SetWorkItems(_cachedWorkItems, Users);

            UpdateOutput();
        }

        private void UpdateOutput()
        {
            var markdown = _dailyScrum.GenerateReport(Users.Where(x => x.Included).ToList());
            var html = Markdig.Markdown.ToHtml(markdown);
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

        private static List<User> GetUniqueUsers(IReadOnlyCollection<WorkItem> workItems)
        {
            Dictionary<string, User> users = new();

            if (!workItems.Any())
            {
                return users.Values.ToList();
            }
            
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

        private void EnsureConnectionInfoValid()
        {
            if (ConnectionInfo is null)
            {
                throw new ArgumentNullException(nameof(ConnectionInfo));
            }
            
            ConnectionInfo.EnsureValid();
        }
        
        private string GetGenerateForLabel() => "Generate for " + ConnectionInfoRequest.TeamFilterBy;
        private void OnBlockingUpdated() => UpdateOutput();
    }
}
