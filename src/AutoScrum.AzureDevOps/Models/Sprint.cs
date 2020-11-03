using System;

namespace AutoScrum.AzureDevOps.Models
{
    public class Sprint
    {
		public string Name { get; set; } = string.Empty;
        public Guid Id { get; set; }
    }
}
