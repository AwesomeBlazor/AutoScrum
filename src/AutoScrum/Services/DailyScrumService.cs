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
            WorkItem item = list.FirstOrDefault(x => x.Id == wi.Id);
            if (item != null)
            {
                list.Remove(item);
                return;
            }

            item = list.FirstOrDefault(x => x.Id == wi.ParentId);
            if (item != null)
            {
                list.Remove(item);
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

        public string GeneratePlainTextReport()
        {
            // All days except for Monday will have "Yesterday", otherwise "Friday".
            // NOTE: MVP doesn't support flexible dates like not working on project for X day and then coming back. (or work on weekends)
            string previousDay = "Yesterday";
            var diff = _dateService.GetPreviousWorkData(TodayDate).Subtract(TodayDate).TotalDays;
            if (TodayDate.Subtract(_dateService.GetPreviousWorkData(TodayDate)).TotalDays > 1)
            {
                previousDay = "Friday";
            }

            string output = GenerateDayReport(previousDay, Yesterday);
            if (string.IsNullOrWhiteSpace(output))
            {
                output += Environment.NewLine;
            }

            output += GenerateDayReport("Today", Today);

            return output;
        }

        public static string GenerateDayReport(string day, List<WorkItem> workItems)
        {
            if (workItems.Any())
            {
                string report = $"{day}{Environment.NewLine}";
                foreach (var wi in workItems)
                {
                    report += $"  - {wi.State} - {(wi.Type == "Task" ? "Task " : "" )}#{wi.Id}: {wi.Title}{Environment.NewLine}";

                    foreach (var child in wi.Children)
                    {
                        report += $"    - {child.State} - {child.Title}{Environment.NewLine}";
                    }
                }

                return report;
            }

            return string.Empty;
        }
    }
}
