using System;

namespace AutoScrum.Core.Models
{
    public class Sprint
    {
        public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }
}
