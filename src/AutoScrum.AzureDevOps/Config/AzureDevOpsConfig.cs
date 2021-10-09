using AutoScrum.Core.Config;
using System;

namespace AutoScrum.AzureDevOps.Config
{
    public class AzureDevOpsConfig
    {
        public string Organization { get; set; }
        public Uri OrganizationUrl { get; set; }
        public string Project { get; set; }
        public string UserEmail { get; set; }
        public string Token { get; set; }

        public static AzureDevOpsConfig FromConfig(ProjectConfigAzureDevOps config)
            => new()
            {
                UserEmail = config.UserEmail,
                Organization = config.AzureDevOpsOrganization,
                OrganizationUrl = new Uri($"https://{config.AzureDevOpsOrganization}.visualstudio.com"),
                Project = config.ProjectName,
                Token = config.PersonalAccessToken
            };
    }
}
