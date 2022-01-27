using System;
using System.Collections.Generic;

namespace AutoScrum.Core.Models
{
    public class WorkItem
    {
        private string? _blocked;

        public int? Id { get; set; }
        public string? IterationPath { get; set; }
        public string? Type { get; set; }
        public string? State { get; set; }
        public string? Title { get; set; }
        public string? AssignedToDisplayName { get; set; }
        public string? AssignedToEmail { get; set; }
        public DateTimeOffset? ChangedDate { get; set; }
        public DateTimeOffset? StateChangeDate { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Blocked
        {
            get { return _blocked; }
            set
            {
                _blocked = value;

                // It turns out the value is either null/empty or "Yes"...
                IsBlocked = !string.IsNullOrEmpty(_blocked);
            }
        }
        public bool IsBlocked { get; set; }

        public WorkItem? Parent { get; set; } = null;
        public List<WorkItem> Children { get; set; } = new List<WorkItem>();

        public int? ParentId { get; set; }
        public bool HasParent => ParentId.HasValue;

        public StateType StateType
            => State?.ToUpperInvariant() switch
            {
                "IN PROGRESS" => StateType.InProgress,
                "DONE" => StateType.Done,
                "COMMITTED" => StateType.Committed,
                "APPROVED" => StateType.Approved,
                _ => StateType.NotStarted,
            };

        public WorkItemType WorkItemType
            => Type?.ToUpperInvariant() switch
            {
                "BUG" => WorkItemType.Bug,
                "TASK" => WorkItemType.Task,
                "USER STORY" => WorkItemType.UserStory,
                _ => WorkItemType.PBI
            };

        public WorkItem ShallowClone()
            => new()
            {
                Id = Id,
                IterationPath = IterationPath,
                Type = Type,
                State = State,
                Title = Title,
                StateChangeDate = StateChangeDate,
                Url = Url,
                ParentId = ParentId,
                ChangedDate = ChangedDate,
                AssignedToDisplayName = AssignedToDisplayName,
                AssignedToEmail = AssignedToEmail,
                Blocked = Blocked
            };

        public string TypeCss => WorkItemType switch
        {
            WorkItemType.Bug => "bug",
            WorkItemType.Task => "task",
            WorkItemType.PBI => "pbi",
            WorkItemType.UserStory => "user-story",
            _ => ""
        };
    }
}
