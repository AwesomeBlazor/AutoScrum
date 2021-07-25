using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using Microsoft.TeamFoundation.Work.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using WorkItemModel = AutoScrum.AzureDevOps.Models.WorkItem;

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
            if ((result?.IsSuccessStatusCode) != true)
            {
                return null;
            }

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
                    Path = x.Path
                })
                .FirstOrDefault();

            return currentSprint;
        }

        public async Task<List<WorkItemModel>?> GetWorkItemsForSprint(Guid sprintId)
        {
            HttpResponseMessage? result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/work/teamsettings/iterations/{sprintId}/workitems?api-version=6.0-Preview.1"));
            if ((result?.IsSuccessStatusCode) != true)
            {
                return null;
            }

            string? json = await result.Content.ReadAsStringAsync();
            var iterationWis = JsonConvert.DeserializeObject<IterationWorkItems>(json);

            List<int>? wiIds = iterationWis.WorkItemRelations
                .Where(x => x.Rel == null && x.Target != null)
                .Select(x => x.Target.Id)
                .ToList();

            return await GetWorkItems(wiIds);
        }

        public async Task<List<WorkItemModel>?> GetWorkItemsForSprintForMe(Sprint sprint)
        {
            HttpResponseMessage? result = await _httpClient.PostAsJsonAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/wit/wiql?api-version=6.0"),
            new
            {
                // The [System.Iteration] doesn't seem to work for some reason...
                query = $"SELECT [State], [Title] FROM WorkItems WHERE [Assigned to] = @Me AND [System.IterationPath] = '{sprint.Path}' ORDER BY [State] Asc, [Changed Date] Desc"
            });

            if ((result?.IsSuccessStatusCode) != true)
            {
                return null;
            }

            string? json = await result.Content.ReadAsStringAsync();
            var iterationWis = JsonConvert.DeserializeObject<AzureDevOpsWiqlResult>(json);

            List<int>? wiIds = iterationWis.WorkItems
                .Select(x => x.Id)
                .ToList();

            return await GetWorkItems(wiIds);
        }

        public async Task<List<WorkItemModel>?> GetWorkItems(IEnumerable<int> ids)
        {
            string idsAsString = string.Join(",", ids.Select(x => x.ToString()));
            HttpResponseMessage? result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/wit/workitems?ids={idsAsString}&api-version=6.0"));
            if ((result?.IsSuccessStatusCode) != true)
            {
                return null;
            }

            string? json = await result.Content.ReadAsStringAsync();
            var devOpsWorkItems = JsonConvert.DeserializeObject<AzureDevOpsListResult<WorkItem>>(json);

            List<WorkItemModel>? workItems = devOpsWorkItems
                ?.Value
                .Select(x => new WorkItemModel
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
    }
}
