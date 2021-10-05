using System;
using System.Collections.Generic;
using System.Linq;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.Models;

namespace AutoScrum.Services
{
    public class DailyScrumService
    {
        private readonly DateService _dateService = new();

        public DailyScrumService()
        {
            TodayMidnight = _dateService.GetDateTimeLocal();
            TodayDay = _dateService.GetToday();
        }

        public List<PersonDailyScrum> TeamsDailyScrum { get; } = new();

        public List<WorkItem> Yesterday { get; } = new();
        public List<WorkItem> Today { get; } = new();
        public List<WorkItem> WorkItems { get; } = new();
        public DateTimeOffset TodayMidnight { get; }
        public DateOnly TodayDay { get; }

        public void SetWorkItems(List<WorkItem> workItems, List<User> users)
        {
            TeamsDailyScrum.Clear();
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
                var parentId = wi.ParentId.Value;
                var parent = GetOrCloneParent(list, parentId);
                if (parent == null)
                {
                    // No parent available, add it to top level.
                    list.Add(wi.ShallowClone());
                }
                else if (!parent.Children.Any(x => x.Id == wi.Id))
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
            if (parent != null)
            {
                item = parent.Children.FirstOrDefault(x => x.Id == wi.Id);
                parent.Children.Remove(item);
            }
        }

        private WorkItem GetOrCloneParent(List<WorkItem> list, int parentId)
        {
            var parent = list.FirstOrDefault(x => x.Id == parentId);
            if (parent == null)
            {
                parent = WorkItems.FirstOrDefault(x => x.Id == parentId);

                if (parent != null)
                {
                    parent = parent?.ShallowClone();
                    list.Add(parent);
                }
            }

            return parent;
        }

        public string GenerateReport(List<User> users, bool isMarkdown = true)
        {
            var yesterday = _dateService.GetPreviousWorkDay(TodayDay);
            return isMarkdown
                ? DailyScrumGenerator.GenerateMarkdownReport(TodayDay, yesterday, Today, Yesterday, users)
                : DailyScrumGenerator.GeneratePlainTextReport(TodayDay, yesterday, Today, Yesterday);
        }
    }
}
