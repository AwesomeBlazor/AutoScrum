using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AntDesign;
using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.AzureDevOps;
using AutoScrum.Core.Models;
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

        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public ConfigService ConfigService { get; set; }
        [Inject] public MessageService MessageService { get; set; }

        private MarkupString Output { get; set; } = (MarkupString)"";

        private AzureDevOpsConnectionInfo ConnectionInfo { get; set; } = new();
        private List<User> Users { get; set; } = new();
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
                await ConfigService.SetConfig(ConnectionInfo);
                MessageService.Success("Config saved successfully!");
            }
            
            _connectionFormLoading = false;
        }

        private async Task DeleteConfigAsync()
        {
            _connectionFormLoading = true;

            await ConfigService.Clear();
            MessageService.Success("Config deleted successfully!");

            _connectionFormLoading = false;
        }

        private async Task GetDataFromAzureDevOpsAsync()
        {
            try
            {
                var devOpsService = new AzureDevOpsService(new AzureDevOpsConfig
                {
                    UserEmail = ConnectionInfo.UserEmail,
                    Organization = ConnectionInfo.AzureDevOpsOrganization,
                    OrganizationUrl = new Uri($"https://{ConnectionInfo.AzureDevOpsOrganization}.visualstudio.com"),
                    Project = ConnectionInfo.ProjectName,
                    Token = ConnectionInfo.PersonalAccessToken
                }, HttpClient);

                var sprint = await devOpsService.GetCurrentSprint();
                Console.WriteLine("Current Sprint: " + sprint?.Name);

                if (sprint != null)
                {
                    var workItems = await devOpsService.GetWorkItemsForSprint(sprint, ConnectionInfo.TeamFilterBy);
                    Users = GetUniqueUsers(workItems);
                    if (!Users.Any())
                    {
                        Users.Add(new User("Me", "me@me.com"));
                    }

                    foreach (var item in workItems)
                    {
                        Console.WriteLine($"{item.Type} {item.Id}: {item.Title}");
                    }

                    _dailyScrum.SetWorkItems(workItems, Users);
                    SelectedUser = Users.FirstOrDefault();

                    UpdateOutput();
                }
                else
                {
                    Console.WriteLine("Unable to load");
                }

                StateHasChanged();
            }
            catch
            {
                MessageService.Error("Critical error while loading data from Azure DevOps");
                throw;
            }
            
        }

        private void UpdateOutput()
        {
            var markdown = _dailyScrum.GenerateReport(Users);
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

        //private void OnSelectedValueChanged(User user)
        //{
        //    if (user != null)
        //    {
        //        SelectedUser = user;
        //        _dailyScrum.ChangeUser(user.Email);

        //        UpdateOutput();
        //    }
        //}
        private string GetGenerateForLabel() => "Generate for " + ConnectionInfo.TeamFilterBy;
    }
}
