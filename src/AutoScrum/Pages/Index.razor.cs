using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.AzureDevOps;
using AutoScrum.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using AutoScrum.Models;

namespace AutoScrum.Pages
{
    public partial class Index
    {
        private readonly DailyScrumService _dailyScrum = new();

        public bool IsPageInitializing { get; set; } = true;

        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public ConfigService ConfigService { get; set; }

        public MarkupString Output { get; set; } = (MarkupString)"";

        public bool ShowUsersAndBlockers { get; set; } = false;
        public bool ShowCurrentSprint { get; set; } = false;
        public bool ShowYesterdayToday { get; set; } = false;

        protected AzureDevOpsConnectionInfo ConnectionInfo { get; set; } = new AzureDevOpsConnectionInfo();

        Validations validations;

        public List<User> Users { get; set; } = new List<User>();
        public User SelectedUser { get; set; }

        protected async override Task OnInitializedAsync()
        {
            try
            {
                var config = await ConfigService.GetConfig();
                if (config != null)
                {
                    ConnectionInfo = config;

                    await GetDataFromAzureDevOps();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await base.OnInitializedAsync();

            IsPageInitializing = false;
        }

        async Task Submit()
        {
            if (validations.ValidateAll())
            {
                validations.ClearAll();
                await GetDataFromAzureDevOps();
            }
            else
            {
                Console.WriteLine("Some validations failed...");
            }
        }

        async Task SaveConfig()
        {
            if (validations.ValidateAll())
            {
                validations.ClearAll();

                await ConfigService.SetConfig(ConnectionInfo);
            }
        }

        async Task DeleteConfig()
        {
            await ConfigService.Clear();
        }

        public async Task GetDataFromAzureDevOps()
        {
            AzureDevOpsService devOpsService = new AzureDevOpsService(new AzureDevOpsConfig
            {
                UserEmail = ConnectionInfo.UserEmail,
                Organization = ConnectionInfo.AzureDevOpsOrganization,
                OrganizationUrl = new Uri($"https://{ConnectionInfo.AzureDevOpsOrganization}.visualstudio.com"),
                Project = ConnectionInfo.ProjectName,
                Token = ConnectionInfo.PersonalAccessToken
            }, HttpClient);

            Sprint sprint = await devOpsService.GetCurrentSprint();
            Console.WriteLine("Current Sprint: " + sprint?.Name);

            if (sprint != null)
            {
                List<WorkItem> workItems = await devOpsService.GetWorkItemsForSprint(sprint, ConnectionInfo.TeamFilterBy);
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

            base.StateHasChanged();
        }

        public void UpdateOutput()
        {
            string markdown = _dailyScrum.GenerateReport(Users);
            string html = Markdig.Markdown.ToHtml(markdown ?? string.Empty);
            Output = (MarkupString)html;

            StateHasChanged();
        }

        public void AddYesterday(WorkItem wi)
        {
            _dailyScrum.AddYesterday(wi);
            UpdateOutput();
        }

        public void AddToday(WorkItem wi)
        {
            _dailyScrum.AddToday(wi);
            UpdateOutput();
        }

        public void RemoveWorkItem(WorkItem wi, bool isToday)
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
            foreach (WorkItem wi in workItems.Where(x => !string.IsNullOrWhiteSpace(x.AssignedToEmail)))
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

            return users.Select(x => x.Value).ToList();
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
    }
}
