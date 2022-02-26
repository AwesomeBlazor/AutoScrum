using System;
using System.Collections.Generic;
using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.AzureDevOps.Utils;
using AutoScrum.Core.Models;
using Microsoft.TeamFoundation.Work.WebApi;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AzureDevOpsWorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using WorkItemModel = AutoScrum.Core.Models.WorkItem;
using AutoScrum.Core.Config;

namespace AutoScrum.AzureDevOps;

/// <summary>
/// Get data from Azure DevOps.
/// NOTE: Microsoft.TeamFoundationServer.Client does not work in Blazor Wasm but we can still use their data structures to save time on building them manually.
/// </summary>
public class AzureDevOpsService
{
    private readonly AzureDevOpsConfig _config;
    private readonly HttpClient _httpClient;

    public AzureDevOpsService(ProjectConfigAzureDevOps config, HttpClient httpClient)
        : this(AzureDevOpsConfig.FromConfig(config), httpClient) { }

    public AzureDevOpsService(AzureDevOpsConfig config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;

        var byteArray = Encoding.ASCII.GetBytes(_config.UserEmail + ":" + _config.Token);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }

    public async Task<Sprint?> GetCurrentSprint()
    {
        var result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/work/teamsettings/iterations?api-version=6.0&$timeframe=current"));

        result.EnsureSuccessStatusCode();

        var json = await result.Content.ReadAsStringAsync();
        var iterations = JsonConvert.DeserializeObject<AzureDevOpsListResult<TeamSettingsIteration>>(json);
        Console.WriteLine("Number of iterations: " + iterations.Count);

        return iterations
            .Value
            .Take(1)
            .Select(x => new Sprint
            {
                Id = x.Id,
                Name = x.Name,
                Path = x.Path
            })
            .FirstOrDefault();
    }

    public async Task<List<WorkItemModel>?> GetWorkItemsForSprint(Sprint sprint)
    {
        var result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/work/teamsettings/iterations/{sprint.Id}/workitems?api-version=6.0-Preview.1"));
        if (result?.IsSuccessStatusCode != true)
        {
            return null;
        }

        var json = await result.Content.ReadAsStringAsync();
        var iterationWis = JsonConvert.DeserializeObject<IterationWorkItems>(json);

        var wiIds = iterationWis.WorkItemRelations
            .Where(x => x.Rel == null && x.Target != null)
            .Select(x => x.Target.Id)
            .ToList();

        return await GetWorkItems(wiIds);
    }

    public async Task<List<WorkItemModel>> GetWorkItemsForSprint(Sprint sprint, TeamFilterBy teamFilterBy)
    {
        var query = "SELECT [State], [Title] FROM WorkItems WHERE ";
        if (teamFilterBy == TeamFilterBy.Me)
        {
            query += "[Assigned to] = @Me AND ";
        }

        // The [System.Iteration] doesn't seem to work for some reason...
        query += $"[System.IterationPath] = '{sprint.Path}' ORDER BY [State] Asc, [Changed Date] Desc";

        var result = await _httpClient.PostAsJsonAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/wit/wiql?api-version=6.0"), new { query });

        result.EnsureSuccessStatusCode();

        var json = await result.Content.ReadAsStringAsync();
        var iterationWis = JsonConvert.DeserializeObject<AzureDevOpsWiqlResult>(json);

        var wiIds = iterationWis.WorkItems
            .ConvertAll(x => x.Id);

        return await GetWorkItems(wiIds);
    }

    private async Task<List<WorkItemModel>> GetWorkItems(IEnumerable<int> ids, bool enableHierarchy = true, bool includeAssignTo = true, bool includeDetails = false)
    {
        if (ids?.Any() != true)
        {
            return new List<WorkItemModel>();
        }

        string queryFields = GetQueryFields(includeAssignTo, includeDetails);

        // Azure DevOps API only supports 200 items at a time. We can easily chunk them into multiple pages.
        // https://github.com/AwesomeBlazor/AutoScrum/issues/71
        const int pageSize = 200;
        IEnumerable<string> chuckedIds = ids.Chunk(pageSize)
            .Select(idsChunk => string.Join(",", idsChunk.Select(x => x.ToString())));

        // Query each chunk and wait for all to finish.
        // NOTE: Task.WhenAll() doesn't seem to support IEnumerable in Blazor WASM...
        List<Task<List<WorkItemModel>>> tasks = chuckedIds.Select(x => GetWorkItems(x, queryFields)).ToList();
        await Task.WhenAll(tasks);

        // Merge all chunks.
        List<WorkItemModel> workItems = tasks
            .SelectMany(x => x.Result)
            .ToList();

        return !enableHierarchy
            ? workItems
            : HierarchicalRestructure(workItems);
    }

    private async Task<List<WorkItemModel>> GetWorkItems(string idsAsString, string fields)
    {
        var result = await _httpClient.GetAsync(new Uri(_config.OrganizationUrl, $"/DefaultCollection/{_config.Project}/_apis/wit/workitems?ids={idsAsString}&fields={fields}&api-version=6.0"));
        result.EnsureSuccessStatusCode();

        var json = await result.Content.ReadAsStringAsync();
        var devOpsWorkItems = JsonConvert.DeserializeObject<AzureDevOpsListResult<AzureDevOpsWorkItem>>(json);

        return devOpsWorkItems.Value
            .ConvertAll(x => new WorkItemModel
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
                Blocked = x.ParseAsString("Microsoft.VSTS.CMMI.Blocked"),
                Url = $"https://dev.azure.com/{_config.Organization}/{_config.Project}/_workItems/edit/{x.Id}"
            });
    }

    private static List<WorkItemModel> HierarchicalRestructure(List<WorkItemModel> workItems)
    {
        List<WorkItemModel> groupedItems = new();
        foreach (var wi in workItems)
        {
            wi.Parent = workItems.Find(x => x.Id == wi.ParentId);
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

        return groupedItems;
    }

    private static string GetQueryFields(bool includeAssignTo, bool includeDetails)
    {
        // Minimum required.
        string fields = "System.State,System.WorkItemType,System.Parent,System.IterationPath,System.CreatedDate,System.ChangedDate,System.Title,Microsoft.VSTS.Common.StateChangeDate,Microsoft.VSTS.CMMI.Blocked";
        if (includeAssignTo)
        {
            // Get user assigned to the work items. (it's a complex structure)
            fields += ",System.AssignedTo";
        }

        if (includeDetails)
        {
            // Get a lot more details on the work item.
            fields += ",System.Reason,Microsoft.VSTS.Scheduling.Effort,System.Tags,Microsoft.VSTS.Scheduling.RemainingWork,System.RelatedLinks,System.RelatedLinkCount,System.ExternalLinkCount";
        }

        return fields;
    }
}
