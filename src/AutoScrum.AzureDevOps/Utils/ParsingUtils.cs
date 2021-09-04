using System;
using System.Globalization;
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

            return DateTimeOffset.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset date)
                ? (DateTimeOffset?)date
                : default;
        }

        public static string ParsePersonDisplayName(this AzureDevOpsWorkItem wi, string key)
        {
            if (!wi.Fields.TryGetValue(key, out object? value) || value is not Microsoft.VisualStudio.Services.WebApi.IdentityRef identity)
            {
                return default;
            }

            return identity.DisplayName;
        }

        public static string ParsePersonUniqueName(this AzureDevOpsWorkItem wi, string key)
        {
            if (!wi.Fields.TryGetValue(key, out object? value) || value is not Microsoft.VisualStudio.Services.WebApi.IdentityRef identity)
            {
                return default;
            }

            return identity.UniqueName;
        }
    }
}
