using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.AzureDevOps.Utils;
using AutoScrum.Core.Models;
using Microsoft.TeamFoundation.Work.WebApi;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using AzureDevOpsWorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
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

        public async Task<List<WorkItemModel>?> GetWorkItemsForSprint(Sprint sprint)
        {
            HttpResponseMessage? result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/work/teamsettings/iterations/{sprint.Id}/workitems?api-version=6.0-Preview.1"));
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

        public async Task<List<WorkItemModel>?> GetWorkItemsForSprint(Sprint sprint, TeamFilterBy teamFilterBy)
        {
            string query = "SELECT [State], [Title] FROM WorkItems WHERE ";
            if (teamFilterBy == TeamFilterBy.Me)
            {
                query += "[Assigned to] = @Me AND ";
            }

            // The [System.Iteration] doesn't seem to work for some reason...
            query += $"[System.IterationPath] = '{sprint.Path}' ORDER BY [State] Asc, [Changed Date] Desc";

            HttpResponseMessage? result = await _httpClient.PostAsJsonAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/wit/wiql?api-version=6.0"),
            new
            {
                query = query
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

        public async Task<List<WorkItemModel>?> GetWorkItems(IEnumerable<int> ids, bool enableHierarchy = true, bool includeAssignTo = true, bool includeDetails = false)
        {
            string idsAsString = string.Join(",", ids.Select(x => x.ToString()));
            
            // Minimum required.
            string fields = "System.State,System.WorkItemType,System.Parent,System.IterationPath,System.CreatedDate,System.ChangedDate,System.Title,Microsoft.VSTS.Common.StateChangeDate";
            if (includeAssignTo)
            {
                fields += ",System.AssignedTo";
            }

            if (includeDetails)
            {
                fields += ",System.Reason,Microsoft.VSTS.Scheduling.Effort,System.Tags,Microsoft.VSTS.Scheduling.RemainingWork,System.RelatedLinks,System.RelatedLinkCount,System.ExternalLinkCount";
            }


            HttpResponseMessage? result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/wit/workitems?ids={idsAsString}&fields={fields}&api-version=6.0"));
            if ((result?.IsSuccessStatusCode) != true)
            {
                return null;
            }

            string? json = await result.Content.ReadAsStringAsync();
            var devOpsWorkItems = JsonConvert.DeserializeObject<AzureDevOpsListResult<AzureDevOpsWorkItem>>(json);

            List<WorkItemModel>? workItems = devOpsWorkItems
                ?.Value
                .Select(x => new WorkItemModel
                {
                    Id = x.Id,
                    Title = x.ParseAsString("System.Title"),
                    IterationPath = x.ParseAsString("System.IterationPath"),
                    Type = x.ParseAsString("System.WorkItemType"),
                    State = x.ParseAsString("System.State"),
                    StateChangeDate = x.ParseAsDate("Microsoft.VSTS.Common.StateChangeDate"),
                    ChangedDate = x.ParseAsDate("System.ChangedDate"),
                    ParentId = x.ParseAsNullableInt("System.Parent"),
                    AssignedToDisplayName = x.ParsePersonDisplayName("System.AssignedTo"),
                    AssignedToEmail = x.ParsePersonUniqueName("System.AssignedTo"),
                    Url = x.Url
                })
                .ToList();

            List<WorkItemModel>? groupedItems = workItems;
            if (workItems != null && enableHierarchy)
            {
                groupedItems = new List<WorkItemModel>();
                foreach (var wi in workItems)
                {
                    wi.Parent = workItems.FirstOrDefault(x => x.Id == wi.ParentId);
                    if (wi.Parent != null)
                    {
                        wi.Parent.Children.Add(wi);
                    }
                    else
                    {
                        // Only top level items are added.
                        groupedItems.Add(wi);
                    }
                }
            }

            return groupedItems;
        }
    }
}
