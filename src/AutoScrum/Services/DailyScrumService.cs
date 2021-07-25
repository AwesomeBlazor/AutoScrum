using AutoScrum.AzureDevOps.Models;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoScrum.Services
{
    public class DailyScrumService
    {
        private readonly DateService _dateService = new DateService();

        public DailyScrumService()
        {
            TodayDate = _dateService.GetDateTimeUtc();
        }

        public List<WorkItem> Yesterday { get; } = new List<WorkItem>();
        public List<WorkItem> Today { get; } = new List<WorkItem>();
        public List<WorkItem> WorkItems { get; } = new List<WorkItem>();
        public DateTimeOffset TodayDate { get; }

        public void SetWorkItems(List<WorkItem> workItems)
        {
            Yesterday.Clear();
            Today.Clear();
            WorkItems.Clear();

            WorkItems.AddRange(workItems);

            // Check auto-add against all work items.
            var allWorkItems = WorkItems.ToList() ;
            allWorkItems.AddRange(workItems.SelectMany(x => x.Children));

            // All in-progress work items should be added for today.
            // All recently completed work items should be moved to yesterday.
            // All in-progress work items that older than a day, should be added to yesterday.
            var yesterday = _dateService.GetPreviousWorkData(TodayDate);
            foreach (var wi in allWorkItems.Where(x => x.State == "In Progress" || x.State == "Done"))
            {
                bool hasChangedRecently = wi.StateChangeDate > yesterday && wi.StateChangeDate < TodayDate;
                if (wi.State == "In Progress")
                {
                    AddToday(wi);

                    if (wi.StateChangeDate < TodayDate)
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
                int parentId = wi.ParentId.Value;
                WorkItem parent = GetOrCloneParent(list, parentId);
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
            WorkItem item = list.FirstOrDefault(x => x.Id == wi.Id);
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
            WorkItem parent = list.FirstOrDefault(x => x.Id == parentId);
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

        public string GenerateReport(bool isMarkdown = true)
        {
            var yesterday = _dateService.GetPreviousWorkData(TodayDate);
            return isMarkdown
                ? DailyScrumGenerator.GenerateMarkdownReport(TodayDate, yesterday, Today, Yesterday)
                : DailyScrumGenerator.GeneratePlainTextReport(TodayDate, yesterday, Today, Yesterday);
        }
    }
}
