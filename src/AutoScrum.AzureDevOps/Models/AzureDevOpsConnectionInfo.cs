using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AutoScrum.Core.Models;
using Microsoft.VisualStudio.Services.Users;

namespace AutoScrum.AzureDevOps.Models
{
    public class AzureDevOpsConnectionInfo
    {
        public AzureDevOpsConnectionInfo(string userEmail, string personalAccessToken, string azureDevOpsOrganization, string projectName, TeamFilterBy teamFilterBy)
        {
            UserEmail = userEmail;
            PersonalAccessToken = personalAccessToken;
            AzureDevOpsOrganization = azureDevOpsOrganization;
            ProjectName = projectName;
            TeamFilterBy = teamFilterBy;
        }
        
        public string? UserEmail { get; set; }
        public string? PersonalAccessToken { get; set; }
        public string? AzureDevOpsOrganization { get; set; }
        public string? ProjectName { get; set; }
        public TeamFilterBy TeamFilterBy { get; set; }

        public void EnsureValid()
        {
            EnsurePropertyIsValid(x => x.UserEmail, nameof(UserEmail));
            EnsurePropertyIsValid(x => x.PersonalAccessToken, nameof(PersonalAccessToken));
            EnsurePropertyIsValid(x => x.AzureDevOpsOrganization, nameof(AzureDevOpsOrganization));
            EnsurePropertyIsValid(x => x.ProjectName, nameof(ProjectName));
        }

        private void EnsurePropertyIsValid(Func<AzureDevOpsConnectionInfo, string?> property, string paramName)
        {
            if (string.IsNullOrWhiteSpace(property.Invoke(this)))
            {
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null or whitespace");
            }
        }
    }

    public class AzureDevOpsConnectionInfoRequest
    {
        [Required, DisplayName("Email")]
        public string? UserEmail { get; set; }
        [Required, DisplayName("Access Token")]
        public string? PersonalAccessToken { get; set; }
        [Required, DisplayName("Organisation")]
        public string? AzureDevOpsOrganization { get; set; }
        [Required, DisplayName("Project")]
        public string? ProjectName { get; set; }
        public TeamFilterBy TeamFilterBy { get; set; }
    }
}
