using System;

namespace AutoScrum.Services
{
    public class DateService
    {
        public DateTimeOffset GetDateTimeUtc() => DateTimeOffset.UtcNow;

        public DateTimeOffset GetPreviousWorkData(DateTimeOffset day)
        {
            int daysBack = -1;
            DayOfWeek dayOfWeek = day.DayOfWeek;
            if (dayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
            {
                daysBack -= dayOfWeek == DayOfWeek.Sunday ? 1 : 2;
            }

            return day.UtcDateTime.Date.AddDays(daysBack);
        }
    }
}
