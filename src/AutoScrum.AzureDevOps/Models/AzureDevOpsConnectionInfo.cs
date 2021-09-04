using AutoScrum.Core.Models;

namespace AutoScrum.AzureDevOps.Models
{
    public class AzureDevOpsConnectionInfo
    {
        public string? UserEmail { get; set; }
        public string? PersonalAccessToken { get; set; }
        public string? AzureDevOpsOrganization { get; set; }
        public string? ProjectName { get; set; }
        public TeamFilterBy TeamFilterBy { get; set; }
    }
}
