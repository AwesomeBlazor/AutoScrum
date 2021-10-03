using System;
using System.Collections.Generic;
using System.Linq;
using AutoScrum.AzureDevOps.Models;
using AutoScrum.Models;

namespace AutoScrum.Services
{
    public static class DailyScrumGenerator
    {
        public static string GenerateMarkdownReport(DateOnly todayDay, DateOnly previousDay, List<WorkItem> today, List<WorkItem> yesterday, List<User> users)
        {
            bool isTeam = users?.Count > 1;
            string dailyScrumReport = string.Empty;
            if (isTeam)
            {
                dailyScrumReport = "## Team Daily Scrum" + Environment.NewLine + Environment.NewLine;
            }

            foreach (var user in users.Where(x => x.IncludeUser))
            {
                string userDailyScrum = string.Empty;
                // All days except for Monday will have "Yesterday", otherwise "Friday".
                // NOTE: MVP doesn't support flexible dates like not working on project for X day and then coming back. (or work on weekends)
                string previousDayName = "Yesterday";
                if (previousDay.DayOfWeek == DayOfWeek.Friday)
                {
                    previousDayName = "Friday";
                }

                string report = GenerateDayMarkdownReport(previousDayName, yesterday, user.Email);
                if (!string.IsNullOrWhiteSpace(report))
                {
                    userDailyScrum += $"{report}{Environment.NewLine}{Environment.NewLine}";
                }

                report = GenerateDayMarkdownReport("Today", today, user.Email);
                if (!string.IsNullOrWhiteSpace(report))
                {
                    userDailyScrum += $"{report}{Environment.NewLine}{Environment.NewLine}";
                }

                report = GenerateBlockersMarkdown(today, user.Blocking);
                if (!string.IsNullOrWhiteSpace(report))
                {
                    userDailyScrum += $"{report}{Environment.NewLine}{Environment.NewLine}";
                }

                if (!string.IsNullOrWhiteSpace(userDailyScrum))
                {
                    if (isTeam)
                    {
                        userDailyScrum = "### " + user.DisplayName + Environment.NewLine + Environment.NewLine + userDailyScrum;
                    }

                    dailyScrumReport += userDailyScrum;
                }
            }

            return dailyScrumReport;
        }

        private static string GenerateDayMarkdownReport(string day, IEnumerable<WorkItem> workItems, string userEmail)
        {
            if (workItems.Any())
            {
                string report = $"**{day}**{Environment.NewLine}";
                bool hasWork = false;
                foreach (var wi in workItems)
                {
                    var userTaks = wi.Children.Where(x => x.AssignedToEmail == userEmail).ToList();
                    if (wi.AssignedToEmail != null && !userTaks.Any())
                    {
                        continue;
                    }

                    hasWork = true;
                    string state = wi.State;
                    if (wi.State is not ("In Progress" or "Done"))
                    {
                        // Clients prefer this over "Committed" or "Approved".
                        state = "In Progress";
                    }

                    report += $"- {state} - [{wi.Type} {wi.Id}]({wi.Url}): {wi.Title}{Environment.NewLine}";

                    foreach (var child in userTaks)
                    {
                        report += $"   - {child.State} - [{child.Type} {child.Id}]({child.Url}): {child.Title}{Environment.NewLine}";
                    }
                }

                if (hasWork)
                {
                    return report;
                }
            }

            return string.Empty;
        }

        private static string GenerateBlockersMarkdown(IEnumerable<WorkItem> workItems, string blocker)
        {
            var blockedItems = workItems
                .Where(x => x.IsBlocked)
                .ToList();

            blockedItems.AddRange(workItems
                .SelectMany(x => x.Children)
                .Where(x => x.IsBlocked));

            if (!blockedItems.Any() && string.IsNullOrWhiteSpace(blocker))
            {
                return string.Empty;
            }

            string report = $"**Blocking**{Environment.NewLine}";
            if (blockedItems.Any())
            {
                foreach (var wi in blockedItems)
                {
                    report += $"- [{wi.Type} {wi.Id}]({wi.Url}): {wi.Title}{Environment.NewLine}";
                }
            }

            if (!string.IsNullOrWhiteSpace(blocker))
            {
                report += $"- {blocker}{Environment.NewLine}";
            }

            return report;
        }

        public static string GeneratePlainTextReport(DateOnly todayDay, DateOnly previousDay, List<WorkItem> today, List<WorkItem> yesterday)
        {
            // All days except for Monday will have "Yesterday", otherwise "Friday".
            // NOTE: MVP doesn't support flexible dates like not working on project for X day and then coming back. (or work on weekends)
            string previousDayName = "Yesterday";
            if (previousDay.DayOfWeek == DayOfWeek.Friday)
            {
                previousDayName = "Friday";
            }

            string output = GenerateDayPlainTextReport(previousDayName, yesterday);
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
                string report = $"{day}{Environment.NewLine}";
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
