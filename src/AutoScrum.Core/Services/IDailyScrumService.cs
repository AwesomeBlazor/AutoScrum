using AutoScrum.Core.Config;
using AutoScrum.Core.Models;
using System;
using System.Collections.Generic;

namespace AutoScrum.Core.Services;

public interface IDailyScrumService
{
    List<PersonDailyScrum> TeamsDailyScrum { get; }
    List<WorkItem> Yesterday { get; }
    List<WorkItem> Today { get; }
    List<WorkItem> WorkItems { get; }
    DateTimeOffset TodayMidnight { get; }
    DateOnly TodayDay { get; }

    void SetWorkItems(List<WorkItem> workItems, List<User> users);
    void AddToday(WorkItem wi);
    void RemoveToday(WorkItem wi);
    void AddYesterday(WorkItem wi);
    void RemoveYesterday(WorkItem wi);
    string? GenerateReport(List<User> users, ProjectType projectType, ReportOutputType reportOutputType);
}
