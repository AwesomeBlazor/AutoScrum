using AutoScrum.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace AutoScrum.Tests.Services
{
    public class DateServiceTests
    {
        private readonly DateService _dateService = new DateService();

        [Fact]
        public void DateService_should_be_true() => true.Should().BeTrue();

        [Theory]
        [MemberData(nameof(SimpleDates))]
        public void ShouldReturnYesterday(DateTimeOffset today, DateTimeOffset yesterday)
        {
            _dateService.GetPreviousWorkData(today).Should().Be(yesterday);
        }

        [Theory]
        [MemberData(nameof(WeekendAndMonday))]
        public void ShouldReturnPreviousWorkDay(DateTimeOffset today, DateTimeOffset yesterday)
        {
            _dateService.GetPreviousWorkData(today).Should().Be(yesterday);
        }

        public static List<object[]> SimpleDates { get; set; } = new()
        {
            // Tuesday - Friday
            new object[] { new DateTimeOffset(new DateTime(2021, 07, 20, 0, 0, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(2021, 07, 19, 0, 0, 0, DateTimeKind.Utc)) },
            new object[] { new DateTimeOffset(new DateTime(2021, 07, 21, 0, 0, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(2021, 07, 20, 0, 0, 0, DateTimeKind.Utc)) },
            new object[] { new DateTimeOffset(new DateTime(2021, 07, 22, 0, 0, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(2021, 07, 21, 0, 0, 0, DateTimeKind.Utc)) },
            new object[] { new DateTimeOffset(new DateTime(2021, 07, 23, 0, 0, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(2021, 07, 22, 0, 0, 0, DateTimeKind.Utc)) }
        };

        public static List<object[]> WeekendAndMonday { get; set; } = new()
        {
            // Saturday - Monday
            new object[] { new DateTimeOffset(new DateTime(2021, 07, 24, 0, 0, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(2021, 07, 23, 0, 0, 0, DateTimeKind.Utc)) },
            new object[] { new DateTimeOffset(new DateTime(2021, 07, 25, 0, 0, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(2021, 07, 23, 0, 0, 0, DateTimeKind.Utc)) },
            new object[] { new DateTimeOffset(new DateTime(2021, 07, 26, 0, 0, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(2021, 07, 23, 0, 0, 0, DateTimeKind.Utc)) }
        };
    }
}
