using AutoScrum.Core.Config;
using System;

namespace AutoScrum.AzureDevOps.Config;

public class AzureDevOpsConfig
{
    public AzureDevOpsConfig(string organization, Uri organizationUrl, string project, string? userEmail, string token)
    {
        Organization = organization;
        OrganizationUrl = organizationUrl;
        Project = project;
        UserEmail = userEmail;
        Token = token;
    }

    public string Organization { get; set; }
    public Uri OrganizationUrl { get; set; }
    public string Project { get; set; }
    public string? UserEmail { get; set; }
    public string Token { get; set; }

    public static AzureDevOpsConfig FromConfig(ProjectConfigAzureDevOps config)
        => new(
            config.AzureDevOpsOrganization!,
            new Uri($"https://{config.AzureDevOpsOrganization}.visualstudio.com"),
            config.ProjectName!,
            config.UserEmail!,
            config.PersonalAccessToken!);
}
