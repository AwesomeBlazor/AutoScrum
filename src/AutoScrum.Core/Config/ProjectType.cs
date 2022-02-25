namespace AutoScrum.Core.Config;

/// <summary>
/// Used to figure out how to deserialize project config.
/// Atm, we only support Azure DevOps and GitHub is scheduled to implemented soon-ish.
/// 
/// Other possible integrations could GitLab, Jira and maybe more. (provided they have nice APIs)
/// </summary>
public enum ProjectType
{
    AzureDevOps = 0,
    GitHub
}
