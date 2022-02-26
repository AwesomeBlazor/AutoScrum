using System.Collections.Generic;

namespace AutoScrum.Core.Models;

public class PersonDailyScrum
{
    public PersonDailyScrum(List<WorkItem> yesterday, List<WorkItem> today, string userEmail)
    {
        Yesterday = yesterday;
        Today = today;
        UserEmail = userEmail;
    }

    public List<WorkItem> Yesterday { get; set; }
    public List<WorkItem> Today { get; set; }

    public string UserEmail { get; set; }
}
