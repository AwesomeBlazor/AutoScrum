using System;

namespace AutoScrum.Core.Services;

public interface IDateService
{
    TimeOnly Midnight { get; }

    DateTimeOffset GetDateTimeLocal();
    DateTimeOffset GetDateTimeUtc();
    DateTime GetPreviousWorkDate(DateOnly day);
    DateTimeOffset GetPreviousWorkDate(DateTimeOffset day);
    DateOnly GetPreviousWorkDay(DateOnly day);
    DateOnly GetToday();
    DateTime GetTodayMidnight();
}

public class DateService : IDateService
{
    public TimeOnly Midnight { get; } = TimeOnly.FromTimeSpan(TimeSpan.Zero);

    public DateTimeOffset GetDateTimeUtc() => DateTimeOffset.UtcNow;
    public DateTimeOffset GetDateTimeLocal() => DateTimeOffset.Now;
    public DateTime GetTodayMidnight() => DateTime.Now.Date;
    public DateOnly GetToday() => DateOnly.FromDateTime(DateTime.Now);

    public DateTimeOffset GetPreviousWorkDate(DateTimeOffset day)
    {
        var daysBack = -1;
        var dayOfWeek = day.DayOfWeek;
        if (dayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            daysBack -= dayOfWeek == DayOfWeek.Sunday ? 1 : 2;
        }

        return day.UtcDateTime.Date.AddDays(daysBack);
    }

    public DateOnly GetPreviousWorkDay(DateOnly day)
    {
        var daysBack = -1;
        var dayOfWeek = day.DayOfWeek;
        if (dayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            daysBack -= dayOfWeek == DayOfWeek.Sunday ? 1 : 2;
        }

        return day.AddDays(daysBack);
    }

    public DateTime GetPreviousWorkDate(DateOnly day)
    {
        day = GetPreviousWorkDay(day);
        return day.ToDateTime(Midnight, DateTimeKind.Local).ToUniversalTime();
    }
}
