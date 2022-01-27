using AutoScrum.Core.Models;
using AutoScrum.Core.Utils;

namespace AutoScrum.Core.Config;

public class ProjectConfigAzureDevOps : ProjectConfig
{
    public ProjectConfigAzureDevOps()
    {
        ProjectType = ProjectType.AzureDevOps;
    }

    public string? UserEmail { get; set; }
    public string? PersonalAccessToken { get; set; }
    public string? AzureDevOpsOrganization { get; set; }
    public TeamFilterBy TeamFilterBy { get; set; }

    public ProjectConfigAzureDevOps Clone()
        => new()
        {
            UserEmail = UserEmail,
            PersonalAccessToken = PersonalAccessToken,
            AzureDevOpsOrganization = AzureDevOpsOrganization,
            TeamFilterBy = TeamFilterBy,
            ProjectName = ProjectName,
            ProjectType = ProjectType,
            Version = Version
        };

    public void EnsureValid()
    {
        this.EnsurePropertyIsValid(x => x.UserEmail, nameof(UserEmail));
        this.EnsurePropertyIsValid(x => x.PersonalAccessToken, nameof(PersonalAccessToken));
        this.EnsurePropertyIsValid(x => x.AzureDevOpsOrganization, nameof(AzureDevOpsOrganization));
        this.EnsurePropertyIsValid(x => x.ProjectName, nameof(ProjectName));
    }
}