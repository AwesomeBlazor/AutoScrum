using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoScrum.Core.Models;
using AutoScrum.Models;

namespace AutoScrum.Services
{
    public static class DailyScrumGenerator
    {
        public static string GenerateMarkdownReport(DateOnly todayDay, DateOnly previousDay, List<WorkItem> today, List<WorkItem> yesterday, List<User> users)
        {
            var isTeam = users?.Count > 1;
            var dailyScrumReport = new StringBuilder();
            if (isTeam)
            {
                dailyScrumReport.Append("## Team Daily Scrum" + Environment.NewLine + Environment.NewLine);
            }

            foreach (var user in users.Where(x => x.Included))
            {
                var userDailyScrum = new StringBuilder();
                // All days except for Monday will have "Yesterday", otherwise "Friday".
                // NOTE: MVP doesn't support flexible dates like not working on project for X day and then coming back. (or work on weekends)
                var previousDayName = "Yesterday";
                if (previousDay.DayOfWeek == DayOfWeek.Friday)
                {
                    previousDayName = "Friday";
                }

                var report = GenerateDayMarkdownReport(previousDayName, yesterday, user.Email);
                AddReportSectionIfNotEmpty(userDailyScrum, report);

                report = GenerateDayMarkdownReport("Today", today, user.Email);
                AddReportSectionIfNotEmpty(userDailyScrum, report);

                report = GenerateBlockersMarkdown(today, user.Blocking);
                AddReportSectionIfNotEmpty(userDailyScrum, report);

                if (userDailyScrum.Length > 0)
                {
                    if (isTeam)
                    {
                        userDailyScrum.Insert(0, "### " + user.DisplayName + Environment.NewLine + Environment.NewLine);
                    }

                    dailyScrumReport.Append(userDailyScrum);
                }
            }

            return dailyScrumReport.ToString();
        }

        private static void AddReportSectionIfNotEmpty(StringBuilder userDailyScrum, StringBuilder report)
        {
            if (report != null && report.Length > 0)
            {
                userDailyScrum.Append(report);
                userDailyScrum.Append($"{Environment.NewLine}{Environment.NewLine}");
            }
        }

        private static StringBuilder GenerateDayMarkdownReport(string day, IEnumerable<WorkItem> workItems, string userEmail)
        {
            if (workItems.Any())
            {
                var report = new StringBuilder($"**{day}**{Environment.NewLine}");
                var hasWork = false;
                foreach (var wi in workItems)
                {
                    var userTaks = wi.Children.Where(x => x.AssignedToEmail == userEmail).ToList();
                    if (wi.AssignedToEmail != null && !userTaks.Any())
                    {
                        continue;
                    }

                    hasWork = true;
                    var state = wi.State;
                    if (wi.State is not ("In Progress" or "Done"))
                    {
                        // Clients prefer this over "Committed" or "Approved".
                        state = "In Progress";
                    }

                    report.Append($"- {state} - [{wi.Type} {wi.Id}]({wi.Url}): {wi.Title}{Environment.NewLine}");

                    foreach (var child in userTaks)
                    {
                        report.Append($"   - {child.State} - [{child.Type} {child.Id}]({child.Url}): {child.Title}{Environment.NewLine}");
                    }
                }

                if (hasWork)
                {
                    return report;
                }
            }

            return null;
        }

        private static StringBuilder GenerateBlockersMarkdown(IEnumerable<WorkItem> workItems, string blocker)
        {
            var blockedItems = workItems
                .Where(x => x.IsBlocked)
                .ToList();

            blockedItems.AddRange(workItems
                .SelectMany(x => x.Children)
                .Where(x => x.IsBlocked));

            if (!blockedItems.Any() && string.IsNullOrWhiteSpace(blocker))
            {
                return null;
            }

            var report = new StringBuilder($"**Blocking**{Environment.NewLine}");
            if (blockedItems.Any())
            {
                foreach (var wi in blockedItems)
                {
                    report.Append($"- [{wi.Type} {wi.Id}]({wi.Url}): {wi.Title}{Environment.NewLine}");
                }
            }

            if (!string.IsNullOrWhiteSpace(blocker))
            {
                report.Append($"- {blocker}{Environment.NewLine}");
            }

            return report;
        }

        public static string GeneratePlainTextReport(DateOnly todayDay, DateOnly previousDay, List<WorkItem> today, List<WorkItem> yesterday)
        {
            // All days except for Monday will have "Yesterday", otherwise "Friday".
            // NOTE: MVP doesn't support flexible dates like not working on project for X day and then coming back. (or work on weekends)
            var previousDayName = "Yesterday";
            if (previousDay.DayOfWeek == DayOfWeek.Friday)
            {
                previousDayName = "Friday";
            }

            var output = GenerateDayPlainTextReport(previousDayName, yesterday);
            if (!string.IsNullOrWhiteSpace(output))
            {
                output += Environment.NewLine;
            }

            output += GenerateDayPlainTextReport("Today", today);

            return output;
        }

        private static string GenerateDayPlainTextReport(string day, List<WorkItem> workItems)
        {
            if (workItems.Any())
            {
                var report = $"{day}{Environment.NewLine}";
                foreach (var wi in workItems)
                {
                    report += $"  - {wi.State} - {(wi.WorkItemType == WorkItemType.Task ? "Task " : "")}#{wi.Id}: {wi.Title}{Environment.NewLine}";

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
