﻿using AutoScrum.AzureDevOps.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoScrum.Services
{
    public class DailyScrumService
    {
        public List<WorkItem> Yesterday { get; } = new List<WorkItem>();
        public List<WorkItem> Today { get; } = new List<WorkItem>();
        public List<WorkItem> WorkItems { get; } = new List<WorkItem>();

        public void SetWorkItems(List<WorkItem> workItems)
        {
            Yesterday.Clear();
            Today.Clear();
            WorkItems.Clear();

            WorkItems.AddRange(workItems);

            foreach (var wi in workItems.SelectMany(x => x.Children).Where(x => x.State == "In Progress"))
            {
                AddToday(wi);
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
            string output = GenerateDayReport("Yesterday", Yesterday);
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
