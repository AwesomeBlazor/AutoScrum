using System.Collections.Generic;

namespace AutoScrum.AzureDevOps.Models
{
    public class AzureDevOpsListResult<T>
    {
        public int Count { get; set; }
        public List<T> Value { get; set; } = new List<T>();
    }
}
