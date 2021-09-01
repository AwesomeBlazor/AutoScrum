using AutoScrum.Services;
using AutoScrum.Tests.Utils;
using FluentAssertions;
using System.Globalization;
using Xunit;

namespace AutoScrum.Tests.Services
{
    public class DateServiceTests
    {
        private readonly DateService _dateService = new DateService();

        [Fact]
        public void DateService_should_be_true() => true.Should().BeTrue();


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

        [Theory]
        [MemberData(nameof(SimpleDayMidnights))]
        public void ShouldReturnYesterdayMidnight(DateOnly today, DateTime yesterday)
        {
            using var _ = FakeCultureInfo.SetCulture(CultureInfo.CreateSpecificCulture("en-AU"));

            _dateService.GetPreviousWorkDate(today).Should().Be(yesterday);
        }

        [Theory]
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
            new object[] { DateOnly.Parse("2021-07-20"), new DateTime(2021, 07, 18, 14, 0, 0, DateTimeKind.Utc) },
            new object[] { DateOnly.Parse("2021-07-21"), new DateTime(2021, 07, 19, 14, 0, 0, DateTimeKind.Utc) },
            new object[] { DateOnly.Parse("2021-07-22"), new DateTime(2021, 07, 20, 14, 0, 0, DateTimeKind.Utc) },
            new object[] { DateOnly.Parse("2021-07-23"), new DateTime(2021, 07, 21, 14, 0, 0, DateTimeKind.Utc) }
        };

        public static List<object[]> WeekendAndMondayMidnight { get; set; } = new()
        {
            // Saturday - Monday
            new object[] { DateOnly.Parse("2021-07-24"), new DateTime(2021, 07, 22, 14, 0, 0, DateTimeKind.Utc) },
            new object[] { DateOnly.Parse("2021-07-25"), new DateTime(2021, 07, 22, 14, 0, 0, DateTimeKind.Utc) },
            new object[] { DateOnly.Parse("2021-07-26"), new DateTime(2021, 07, 22, 14, 0, 0, DateTimeKind.Utc) }
        };
    }
}
