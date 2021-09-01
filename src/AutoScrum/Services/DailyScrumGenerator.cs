using AutoScrum.AzureDevOps.Models;

namespace AutoScrum.Services
{
    public static class DailyScrumGenerator
    {
        public static string GenerateMarkdownReport(DateOnly todayDay, DateOnly previousDay, List<WorkItem> today, List<WorkItem> yesterday)
        {
            // All days except for Monday will have "Yesterday", otherwise "Friday".
            // NOTE: MVP doesn't support flexible dates like not working on project for X day and then coming back. (or work on weekends)
            string previousDayName = "Yesterday";
            if (previousDay.DayOfWeek == DayOfWeek.Friday)
            {
                previousDayName = "Friday";
            }

            string output = GenerateDayMarkdownReport(previousDayName, yesterday);
            if (!string.IsNullOrWhiteSpace(output))
            {
                output += $"{Environment.NewLine}{Environment.NewLine}";
            }

            output += GenerateDayMarkdownReport("Today", today);

            return output;
        }

        private static string GenerateDayMarkdownReport(string day, List<WorkItem> workItems)
        {
            if (workItems.Any())
            {
                string report = $"**{day}**{Environment.NewLine}";
                foreach (var wi in workItems)
                {
                    string state = wi.State;
                    if (wi.State is not ("In Progress" or "Done"))
                    {
                        // Clients prefer this over "Committed" or "Approved".
                        state = "In Progress";
                    }

                    report += $"- {state} - [{wi.Type} {wi.Id}]({wi.Url}): {wi.Title}{Environment.NewLine}";

                    foreach (var child in wi.Children)
                    {
                        report += $"   - {child.State} - [{child.Type} {child.Id}]({child.Url}): {child.Title}{Environment.NewLine}";
                    }
                }

                return report;
            }

            return string.Empty;
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
