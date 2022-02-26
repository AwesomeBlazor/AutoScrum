using System;
using System.Collections.Generic;
using AutoScrum.Core.Services;
using AutoScrum.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoScrum.Tests.Services;

public class DateServiceTests : TestBase
{
    private readonly DateService _dateService = new();
    
    [Theory]
    [MemberData(nameof(SimpleDays))]
    public void ShouldReturnYesterday(DateOnly today, DateOnly yesterday)
    {
        _dateService.GetPreviousWorkDay(today).Should().Be(yesterday);
    }

    [Theory]
    [MemberData(nameof(WeekendAndMonday))]
    public void ShouldReturnFriday(DateOnly today, DateOnly yesterday)
    {
        _dateService.GetPreviousWorkDay(today).Should().Be(yesterday);
    }

    [Theory(Skip = "Need to figure out how to test this in different timezones (changing timezone globally doesn't seem to work)")]
    [MemberData(nameof(SimpleDayMidnights))]
    public void ShouldReturnYesterdayMidnight(DateOnly today, DateTime yesterday)
    {
        _dateService.GetPreviousWorkDate(today).Should().Be(yesterday);
    }

    [Theory(Skip = "Need to figure out how to test this in different timezones (changing timezone globally doesn't seem to work)")]
    [MemberData(nameof(WeekendAndMondayMidnight))]
    public void ShouldReturnFridayMidnight(DateOnly today, DateTime yesterday)
    {
        _dateService.GetPreviousWorkDate(today).Should().BeSameDateAs(yesterday);
    }

    public static List<object[]> SimpleDays { get; set; } = new()
    {
        // Tuesday - Friday
        new object[] { DateOnly.Parse("2021-07-20"), DateOnly.Parse("2021-07-19") },
        new object[] { DateOnly.Parse("2021-07-21"), DateOnly.Parse("2021-07-20") },
        new object[] { DateOnly.Parse("2021-07-22"), DateOnly.Parse("2021-07-21") },
        new object[] { DateOnly.Parse("2021-07-23"), DateOnly.Parse("2021-07-22") }
    };

    public static List<object[]> WeekendAndMonday { get; set; } = new()
    {
        // Saturday - Monday
        new object[] { DateOnly.Parse("2021-07-24"), DateOnly.Parse("2021-07-23") },
        new object[] { DateOnly.Parse("2021-07-25"), DateOnly.Parse("2021-07-23") },
        new object[] { DateOnly.Parse("2021-07-26"), DateOnly.Parse("2021-07-23") }
    };

    public static List<object[]> SimpleDayMidnights { get; set; } = new()
    {
        // Tuesday - Friday
        new object[] { DateOnly.Parse("2021-07-20"), TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 07, 19)) },
        new object[] { DateOnly.Parse("2021-07-21"), TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 07, 20)) },
        new object[] { DateOnly.Parse("2021-07-22"), TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 07, 21)) },
        new object[] { DateOnly.Parse("2021-07-23"), TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 07, 22)) }
    };

    public static List<object[]> WeekendAndMondayMidnight { get; set; } = new()
    {
        // Saturday - Monday
        new object[] { DateOnly.Parse("2021-07-24"), TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 07, 23)) },
        new object[] { DateOnly.Parse("2021-07-25"), TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 07, 23)) },
        new object[] { DateOnly.Parse("2021-07-26"), TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 07, 23)) }
    };
}
