using System;
using System.Collections.Generic;

namespace AutoScrum.AzureDevOps.Models
{
    public class WorkItem
    {
        public int? Id { get; set; }
        public string? IterationPath { get; set; }
        public string? Type { get; set; }
        public string? State { get; set; }
        public string? Title { get; set; }
        public string? StateChangeDateString { get; set; }
		public string Url { get; set; } = string.Empty;
        public WorkItem? Parent { get; set; } = null;
        public List<WorkItem> Children { get; set; } = new List<WorkItem>();

        public bool IsBug => Type?.Equals("Bug", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsTask => Type?.Equals("Task", StringComparison.OrdinalIgnoreCase) ?? false;
        public int? ParentId { get; set; }
        public bool HasParent => ParentId.HasValue;

        public WorkItem ShallowClone()
            => new()
            {
                Id = Id,
                IterationPath = IterationPath,
                Type = Type,
                State = State,
                Title = Title,
                StateChangeDateString = StateChangeDateString,
                Url = Url,
                ParentId = ParentId,
            };

        public string TypeCss => Type switch
        {
            null => "",
            _ when Type.Equals("Bug", StringComparison.OrdinalIgnoreCase) => "bug",
            _ when Type.Equals("Task", StringComparison.OrdinalIgnoreCase) => "task",
            _ when Type.Equals("Product Backlog Item", StringComparison.OrdinalIgnoreCase) => "pbi",
            _ when Type.Equals("User Story", StringComparison.OrdinalIgnoreCase) => "user-story",
            _ => ""
        };
    }
}
