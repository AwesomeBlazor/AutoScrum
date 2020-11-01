using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Newtonsoft.Json;

namespace AutoScrum.AzureDevOps
{
    /// <summary>
    /// Get data from Azure DevOps.
    /// NOTE: Microsoft.TeamFoundationServer.Client does not work in Blazor Wasm but we can still use their data structures to save time on building them manually.
    /// </summary>
    public class AzureDevOpsService
    {
        private readonly AzureDevOpsConfig _config;
        private readonly HttpClient _httpClient;

        public AzureDevOpsService(AzureDevOpsConfig config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;

            byte[]? byteArray = Encoding.ASCII.GetBytes(_config.UserEmail + ":" + _config.Token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<Sprint?> GetCurrentSprint()
        {
            HttpResponseMessage? result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/work/teamsettings/iterations?api-version=6.0&$timeframe=current"));
            if (result?.IsSuccessStatusCode == true)
            {
                string? json = await result.Content.ReadAsStringAsync();
                var iterations = JsonConvert.DeserializeObject<AzureDevOpsListResult<TeamSettingsIteration>>(json);
                Console.WriteLine("Number of iterations: " + iterations?.Count);

                Sprint? currentSprint = iterations
                    ?.Value
                    ?.Take(1)
                    .Select(x => new Sprint
                    {
                        Id = x.Id,
                        Name = x.Name,
                    })
                    .FirstOrDefault();

                return currentSprint;
            }

            return null;
        }

        public async Task<List<DevOpsWorkItem>?> GetWorkItemsForSprint(Guid sprintId, string? userEmail = null)
        {
            HttpResponseMessage? result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/work/teamsettings/iterations/{sprintId}/workitems?api-version=6.0-Preview.1"));
            if (result?.IsSuccessStatusCode == true)
            {
                string? json = await result.Content.ReadAsStringAsync();
                var iterationWis = JsonConvert.DeserializeObject<IterationWorkItems>(json);

                List<int>? wiIds = iterationWis.WorkItemRelations
                    .Where(x => x.Rel == null && x.Target != null)
                    .Select(x => x.Target.Id)
                    .ToList();

                return await GetWorkItems(wiIds);
            }

            return null;
        }

        public async Task<List<DevOpsWorkItem>?> GetWorkItems(IEnumerable<int> ids)
        {
            string idsAsString = string.Join(",", ids.Select(x => x.ToString()));
            HttpResponseMessage? result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/wit/workitems?ids={idsAsString}&api-version=6.0"));
            if (result?.IsSuccessStatusCode == true)
            {
                string? json = await result.Content.ReadAsStringAsync();
                var devOpsWorkItems = JsonConvert.DeserializeObject<AzureDevOpsListResult<WorkItem>>(json);

                List<DevOpsWorkItem>? workItems = devOpsWorkItems
                    ?.Value
                    .Select(x => new DevOpsWorkItem
                    {
                        Id = x.Id,
                        Title = x.Fields["System.Title"].ToString(),
                        IterationPath = x.Fields["System.IterationPath"].ToString(),
                        Type = x.Fields["System.WorkItemType"].ToString(),
                        State = x.Fields["System.State"].ToString(),
                        StateChangeDateString = x.Fields["Microsoft.VSTS.Common.StateChangeDate"].ToString(),
                        Url = x.Url
                    })
                    .ToList();

                return workItems;
            }

            return null;
        }
    }
}
