using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoScrum.AzureDevOps;
using AutoScrum.AzureDevOps.Config;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.Models;

namespace AutoScrum.Services
{
    public class DailyScrumService
    {
        private readonly DateService _dateService = new();
        private readonly AutoMessageService _messageService;

        private readonly HttpClient _httpClient = new();

        public DailyScrumService(AutoMessageService messageService)
        {
            _messageService = messageService;
            TodayMidnight = _dateService.GetDateTimeLocal();
            TodayDay = _dateService.GetToday();
        }

        public List<WorkItem> Yesterday { get; } = new();
        public List<WorkItem> Today { get; } = new();
        public List<WorkItem> WorkItems { get; } = new();
        public List<User> Users { get; set; } = new();
        public List<User> IncludedUsers => Users.Where(x => x.Included).ToList();
        public DateTimeOffset TodayMidnight { get; }
        public DateOnly TodayDay { get; }

        public void SetWorkItems(List<WorkItem> workItems, List<User> users)
        {
            Yesterday.Clear();
            Today.Clear();
            WorkItems.Clear();

            WorkItems.AddRange(workItems);

            // Check auto-add against all work items.
            var allWorkItems = WorkItems.ToList();
            allWorkItems.AddRange(workItems.SelectMany(x => x.Children));

            // All in-progress work items should be added for today.
            // All recently completed work items should be moved to yesterday.
            // All in-progress work items that older than a day, should be added to yesterday.
            var yesterday = _dateService.GetPreviousWorkDate(TodayDay);
            foreach (var user in users)
            {
                foreach (var wi in allWorkItems.Where(x => x.StateType is StateType.InProgress or StateType.Done && x.AssignedToEmail == user.Email))
                {
                    var hasChangedRecently = wi.StateChangeDate > yesterday && wi.StateChangeDate < TodayMidnight;
                    if (wi.StateType == StateType.InProgress)
                    {
                        AddToday(wi);

                        if (wi.StateChangeDate < TodayMidnight)
                        {
                            AddYesterday(wi);
                        }
                    }
                    else if (hasChangedRecently)
                    {
                        AddYesterday(wi);
                    }
                }
            }
        }

        public void AddToday(WorkItem wi) => Add(Today, wi);
        public void RemoveToday(WorkItem wi) => Remove(Today, wi);
        public void AddYesterday(WorkItem wi) => Add(Yesterday, wi);
        public void RemoveYesterday(WorkItem wi) => Remove(Yesterday, wi);

        private void Add(List<WorkItem> list, WorkItem wi)
        {
            if (!wi.HasParent)
            {
                if (list.Any(x => x.Id == wi.Id))
                {
                    // Already in the list.
                    return;
                }

                list.Add(wi.ShallowClone());
            }
            else
            {
                var parentId = wi.ParentId!.Value;
                // TODO: Maybe this should return null, because it looks like we have logic
                var parent = GetOrCloneParent(list, parentId);

                if (parent is null)
                {
                    // No parent available, add it to top level.
                    list.Add(wi.ShallowClone());
                }
                else if (parent.Children.All(x => x.Id != wi.Id))
                {
                    parent.Children.Add(wi.ShallowClone());
                }
            }
        }

        private void Remove(List<WorkItem> list, WorkItem wi)
        {
            // Remove the item if on top level.
            var item = list.FirstOrDefault(x => x.Id == wi.Id);
            if (item != null)
            {
                list.Remove(item);
                return;
            }

            // Remove the item from a parent otherwise.
            var parent = list.FirstOrDefault(x => x.Id == wi.ParentId);
            if (parent == null) return;

            item = parent.Children.FirstOrDefault(x => x.Id == wi.Id);
            if (item != null) parent.Children.Remove(item);
        }

        private WorkItem? GetOrCloneParent(List<WorkItem> list, int parentId)
        {
            var parent = list.FirstOrDefault(x => x.Id == parentId);
            if (parent != null) return parent;

            parent = WorkItems.FirstOrDefault(x => x.Id == parentId);

            parent = parent?.ShallowClone();

            if (parent is not null)
            {
                list.Add(parent);
            }

            return parent;
        }

        public void ReloadWorkItems()
        {
            foreach (var item in WorkItems)
            {
                Console.WriteLine($"{item.Type} {item.Id}: {item.Title}");
            }

            SetWorkItems(WorkItems, Users);
        }

        private AzureDevOpsService GetAzureDevOpsService(AzureDevOpsConnectionInfo? connectionInfo)
        {
            EnsureConnectionInfoValid(connectionInfo);

            return new AzureDevOpsService(
                new AzureDevOpsConfig(connectionInfo!.AzureDevOpsOrganization!,
                    new Uri($"https://{connectionInfo.AzureDevOpsOrganization}.visualstudio.com"),
                    connectionInfo.ProjectName!, connectionInfo.UserEmail!, connectionInfo.PersonalAccessToken!),
                _httpClient);
        }

        private async Task<Sprint> GetCurrentSprint(AzureDevOpsConnectionInfo? connectionInfo, AzureDevOpsService? azureDevOpsService = null)
        {
            azureDevOpsService ??= GetAzureDevOpsService(connectionInfo);

            var currentSprint = await azureDevOpsService.GetCurrentSprint();
            Console.WriteLine("Current Sprint: " + currentSprint.Name);

            return currentSprint;
        }

        public async Task GetDataFromAzureDevOpsAsync(AzureDevOpsConnectionInfo? connectionInfo)
        {
            try
            {
                var sprint = await GetCurrentSprint(connectionInfo);

                var workItems = await GetAzureDevOpsService(connectionInfo)
                    .GetWorkItemsForSprint(sprint, connectionInfo!.TeamFilterBy);

                Users = GetUniqueUsers(workItems);
                SetWorkItems(workItems, Users);

                ReloadWorkItems();
            }
            catch
            {
                _messageService.Error("Critical error while loading data from Azure DevOps");
                throw;
            }
        }



        private static List<User> GetUniqueUsers(IReadOnlyCollection<WorkItem> workItems)
        {
            Dictionary<string, User> users = new();

            if (!workItems.Any())
            {
                return users.Values.ToList();
            }

            foreach (var wi in workItems.Where(x => !string.IsNullOrWhiteSpace(x.AssignedToEmail) && !string.IsNullOrWhiteSpace(x.AssignedToDisplayName)))
            {
                if (!users.ContainsKey(wi.AssignedToEmail!))
                {
                    users.Add(wi.AssignedToEmail!, new User(wi.AssignedToDisplayName!, wi.AssignedToEmail!));
                }
            }

            return users.Values.ToList();
        }

        private static void EnsureConnectionInfoValid(AzureDevOpsConnectionInfo? connectionInfo)
        {
            if (connectionInfo is null)
            {
                throw new ArgumentNullException(nameof(connectionInfo));
            }

            connectionInfo.EnsureValid();
        }
    }
}
