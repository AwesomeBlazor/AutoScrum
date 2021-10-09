using AutoScrum.Core.Config;

namespace AutoScrum.Infrastructure.Blazor.Models
{
    internal class ProjectStorageItem
    {
        public ProjectType ProjectType { get; set; }
        public string JsonPayload { get; set; }
    }
}
