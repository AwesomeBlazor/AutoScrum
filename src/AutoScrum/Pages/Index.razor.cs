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

        private Form<AzureDevOpsConnectionInfo> _connectionForm;
        private bool _connectionFormLoading;

        private bool IsPageInitializing { get; set; } = true;

        private List<WorkItem>? _cachedWorkItems;

        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public ConfigService ConfigService { get; set; }
        [Inject] public MessageService MessageService { get; set; }

        private MarkupString Output { get; set; } = (MarkupString)"";

        private AzureDevOpsConnectionInfo ConnectionInfo { get; set; } = new();
        private List<User> Users { get; set; } = new();
        private List<User> IncludedUsers { get; set; } = new();
        private User SelectedUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var config = await ConfigService.GetConfig();
                if (config != null)
                {
                    ConnectionInfo = config;

                    await GetDataFromAzureDevOpsAsync();
                }
            }
            catch
            {
                await MessageService.Error("Critical error while loading config");
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
                await MessageService.Success("Loaded data from Azure DevOps!");
            }
            else
            {
                await MessageService.Warning("Some validations failed...");
            }

            _connectionFormLoading = false;
        }

        private async Task SaveConfigAsync()
        {
            _connectionFormLoading = true;

            if (_connectionForm.Validate())
            {
                await ConfigService.SetConfig(ConnectionInfo);
                await MessageService.Success("Config saved successfully!");
            }
            
            _connectionFormLoading = false;
        }

        private async Task DeleteConfigAsync()
        {
            _connectionFormLoading = true;

            await ConfigService.Clear();
            await MessageService.Success("Config deleted successfully!");

            _connectionFormLoading = false;
        }
        
        private AzureDevOpsService GetAzureDevOpsService() => new(
            new AzureDevOpsConfig
            {
                UserEmail = ConnectionInfo.UserEmail,
                Organization = ConnectionInfo.AzureDevOpsOrganization,
                OrganizationUrl = new Uri($"https://{ConnectionInfo.AzureDevOpsOrganization}.visualstudio.com"),
                Project = ConnectionInfo.ProjectName,
                Token = ConnectionInfo.PersonalAccessToken
            }, HttpClient);

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
                await MessageService.Error("Critical error while loading data from Azure DevOps");
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
