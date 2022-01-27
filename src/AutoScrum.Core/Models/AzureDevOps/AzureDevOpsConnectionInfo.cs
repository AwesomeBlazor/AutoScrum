using AutoScrum.Core.Config;

namespace AutoScrum.Core.Models;

public class AzureDevOpsConnectionInfo
{
    public string? UserEmail { get; set; }
    public string? PersonalAccessToken { get; set; }
    public string? AzureDevOpsOrganization { get; set; }
    public string? ProjectName { get; set; }
    public TeamFilterBy TeamFilterBy { get; set; }

    public static AzureDevOpsConnectionInfo FromConfig(ProjectConfigAzureDevOps config)
        => new()
        {
            UserEmail = config.UserEmail,
            PersonalAccessToken = config.PersonalAccessToken,
            AzureDevOpsOrganization = config.AzureDevOpsOrganization,
            ProjectName = config.ProjectName,
            TeamFilterBy = config.TeamFilterBy
        };
}
