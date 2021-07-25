using System;
using AzureDevOpsWorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace AutoScrum.AzureDevOps.Utils
{
    public static class ParsingUtils
    {
        public static string? ParseAsString(this AzureDevOpsWorkItem wi, string key)
        {
            return wi.Fields.TryGetValue(key, out object? value)
                ? (value?.ToString())
                : default;
        }

        public static int? ParseAsNullableInt(this AzureDevOpsWorkItem wi, string key)
        {
            if (!wi.Fields.TryGetValue(key, out object? value))
            {
                return default;
            }

            return value != null && int.TryParse(value.ToString(), out int number)
                ? number
                : null;
        }

        public static DateTimeOffset? ParseAsDate(this AzureDevOpsWorkItem wi, string key)
        {
            if (!wi.Fields.TryGetValue(key, out object? value))
            {
                return default;
            }

            return DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset date)
                ? (DateTimeOffset?)date
                : default;
        }
    }
}
