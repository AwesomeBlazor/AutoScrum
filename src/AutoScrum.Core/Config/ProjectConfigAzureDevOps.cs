using AutoScrum.Core.Models;

namespace AutoScrum.Core.Config
{
    public class ProjectConfigAzureDevOps : ProjectConfig
    {
        public string? UserEmail { get; set; }
        public string? PersonalAccessToken { get; set; }
        public string? AzureDevOpsOrganization { get; set; }
        public string? ProjectName { get; set; }
        public TeamFilterBy TeamFilterBy { get; set; }
    }
}
