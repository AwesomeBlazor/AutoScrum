using AutoScrum.Core.Config;
using AutoScrum.Core.Models;
using System;
using System.Collections.Generic;

namespace AutoScrum.Core.Services;

public interface IDailyScrumGenerator
{
    ProjectType ProjectType { get; }

    string GenerateReport(DateOnly todayDay, DateOnly previousDay, List<WorkItem> today, List<WorkItem> yesterday, List<User> users, ReportOutputType reportOutputType);
}
