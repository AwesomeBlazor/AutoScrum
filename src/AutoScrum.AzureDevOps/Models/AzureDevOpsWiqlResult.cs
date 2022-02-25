using System;
using System.Collections.Generic;

namespace AutoScrum.AzureDevOps.Models;

public class AzureDevOpsWiqlResult
{
    public string QueryType { get; set; } = string.Empty;
    public string QueryResultType { get; set; } = string.Empty;
    public DateTimeOffset AsOf { get; set; }
    public List<WorkItemLink> WorkItems { get; set; } = new();
}
