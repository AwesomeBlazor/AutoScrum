using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.AzureDevOps;
using AutoScrum.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace AutoScrum.Pages
{
    public partial class Index
    {
        private readonly DailyScrumService _dailyScrum = new();

        [Inject] public ConfigService ConfigService { get; set; }

        public MarkupString Output { get; set; } = (MarkupString)"";

        protected AzureDevOpsConnectionInfo ConnectionInfo { get; set; } = new AzureDevOpsConnectionInfo();

        Validations validations;

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
            }, _httpClient);

            Sprint sprint = await devOpsService.GetCurrentSprint();
            Console.WriteLine("Current Sprint: " + sprint?.Name);

            if (sprint != null)
            {
                var workItems = await devOpsService.GetWorkItemsForSprintForMe(sprint);

                foreach (var item in workItems)
                {
                    Console.WriteLine($"{item.Type} {item.Id}: {item.Title}");
                }

                _dailyScrum.SetWorkItems(workItems);
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
            string markdown = _dailyScrum.GenerateReport();
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
    }
}
