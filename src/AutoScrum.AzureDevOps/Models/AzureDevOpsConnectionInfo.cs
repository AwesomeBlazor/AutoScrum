using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AutoScrum.Core.Models;

namespace AutoScrum.AzureDevOps.Models
{
    public class AzureDevOpsConnectionInfo
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
        
        [Obsolete] public bool TeamFilterByBool { get; set; }
    }
}
