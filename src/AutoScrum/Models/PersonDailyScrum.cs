using AutoScrum.AzureDevOps.Models;

namespace AutoScrum.Models;

public class PersonDailyScrum
{
    public List<WorkItem> Yesterday { get; set; }
    public List<WorkItem> Today { get; set; }

    public string UserEmail { get; set; }
}
