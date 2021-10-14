using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        private const int ContentSpan = 21;
        private const int AnchorSpan = 3;
        private bool IsPageInitializing { get; set; } = true;
        [Inject] public HttpClient HttpClient { get; set; } = null!;
        [Inject] public ConfigService ConfigService { get; set; } = null!;
        [Inject] public AutoMessageService MessageService { get; set; } = null!;
        [Inject] public DailyScrumService DailyScrum { get; set; } = null!;
        [Inject] public OutputService OutputService { get; set; } = null!;
        
        private AzureDevOpsConnectionInfo? ConnectionInfo { get; set; }
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
            
            await DailyScrum.GetDataFromAzureDevOpsAsync(ConnectionInfo);
            OutputService.Update();
            
            IsPageInitializing = false;
        }
    }
}
