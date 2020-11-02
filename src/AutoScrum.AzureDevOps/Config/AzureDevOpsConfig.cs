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
    }
}
