using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.AzureDevOps;
using AutoScrum.Services;
using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoScrum.Pages
{
    public partial class Index
    {
        
        private readonly DailyScrumService _dailyScrum = new DailyScrumService();
        public string Output { get; set; } = "";

        protected AzureDevOpsConnectionInfo ConnectionInfo { get; set; } = new AzureDevOpsConnectionInfo();

        Validations validations;

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
                Output = "Sprint " + sprint.Name + Environment.NewLine;
                var workItems = await devOpsService.GetWorkItemsForSprintForMe(sprint);

                Output += "Your Work items:" + Environment.NewLine;
                foreach (var item in workItems)
                {
                    Console.WriteLine($"{item.Type} {item.Id}: {item.Title}");
                    Output += $"  - {item.Type} {item.Id}: {item.Title}{Environment.NewLine}";
                }

                _dailyScrum.SetWorkItems(workItems);
                UpdateOutput();
            }
            else
            {
                Output = "Unable to load.";
            }

            base.StateHasChanged();
        }

        public void UpdateOutput()
        {
            Output = _dailyScrum.GeneratePlainTextReport();

            this.StateHasChanged();
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

        public static string GenerateDayReport(string day, List<WorkItem> workItems)
        {
            if (workItems.Any())
            {
                string report = string.Join(string.Empty, workItems.Select(wi => $"  - {wi.State} - {wi.Title} ({wi.Type}){Environment.NewLine}"));
                return $"{day}{Environment.NewLine}{report}";
            }

            return string.Empty;
        }
    }
}
