using FluentAssertions;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Tests.Utils.Utils;

public class UtilsExtensionTest
{
    [Theory]
    [InlineData(1609459200000, "2021-01-01T00:00:00Z")]
    [InlineData(1612137600000, "2021-02-01T00:00:00Z")]
    [InlineData(1614556800000, "2021-03-01T00:00:00Z")]
    [InlineData(1622505600000, "2021-06-01T00:00:00Z")]
    [InlineData(1625097600000, "2021-07-01T00:00:00Z")]
    public void ConvertToDatetime_ReturnsCorrectDateTime(long timestamp, string expectedDateTimeString)
    {
        // Arrange
        var expectedDateTime = DateTime.Parse(expectedDateTimeString).ToUniversalTime();

        // Act
        var result = timestamp.ConvertToDatetime();

        // Assert
        result.Should().Be(expectedDateTime);
    }

    [Theory]
    [InlineData("2021-01-01T00:00:00Z", 1609459200000)]
    [InlineData("2021-02-01T00:00:00Z", 1612137600000)]
    [InlineData("2021-03-01T00:00:00Z", 1614556800000)]
    [InlineData("2021-06-01T00:00:00Z", 1622505600000)]
    [InlineData("2021-07-01T00:00:00Z", 1625097600000)]
    public void ConvertToUnixTime_ReturnsCorrectUnixTime(string dateTimeString, long expectedUnixTime)
    {
        // Arrange
        var expectedDateTime = DateTime.Parse(dateTimeString).ToUniversalTime();

        // Act
        var result = expectedDateTime.ConvertToUnixTime();

        // Assert
        Assert.Equal(expectedUnixTime, result);
    }

    [Theory]
    [InlineData(1, DayOfWeek.Monday)]
    [InlineData(2, DayOfWeek.Tuesday)]
    [InlineData(3, DayOfWeek.Wednesday)]
    [InlineData(4, DayOfWeek.Thursday)]
    [InlineData(5, DayOfWeek.Friday)]
    [InlineData(6, DayOfWeek.Saturday)]
    [InlineData(7, DayOfWeek.Sunday)]
    public void GetDay_ReturnsCorrectDayOfWeek(long dayValue, DayOfWeek expectedDayOfWeek)
    {
        // Arrange

        // Act
        var result = RobotAppLibraryV2.Utils.Utils.GetDay(dayValue);

        // Assert
        Assert.Equal(expectedDayOfWeek, result);
    }

    [Fact]
    public void GetDay_WithInvalidValue_ThrowsException()
    {
        // Arrange
        long invalidValue = 8;

        // Act & Assert
        Assert.Throws<Exception>(() => RobotAppLibraryV2.Utils.Utils.GetDay(invalidValue));
    }

    [Theory]
    [InlineData(Timeframe.OneMinute, 1)]
    [InlineData(Timeframe.FiveMinutes, 5)]
    [InlineData(Timeframe.FifteenMinutes, 15)]
    [InlineData(Timeframe.ThirtyMinutes, 30)]
    [InlineData(Timeframe.OneHour, 60)]
    [InlineData(Timeframe.FourHour, 240)]
    [InlineData(Timeframe.Daily, 1440)]
    [InlineData(Timeframe.Weekly, 10080)]
    [InlineData(Timeframe.Monthly, 43800)]
    public void GetMinuteFromTimeframe_ValidTimeframes_ReturnsExpectedMinutes(Timeframe timeFrame, int expected)
    {
        Assert.Equal(expected, timeFrame.GetMinuteFromTimeframe());
    }

    [Fact]
    public void GetMinuteFromTimeframe_InvalidTimeframe_ThrowsException()
    {
        Assert.Throws<Exception>(() => ((Timeframe)999).GetMinuteFromTimeframe());
    }

    [Fact]
    public void TestToPeriodSize()
    {
        Assert.Equal(PeriodSize.OneMinute, Timeframe.OneMinute.ToPeriodSize());
        Assert.Equal(PeriodSize.FiveMinutes, Timeframe.FiveMinutes.ToPeriodSize());
        Assert.Equal(PeriodSize.FifteenMinutes, Timeframe.FifteenMinutes.ToPeriodSize());
        Assert.Equal(PeriodSize.ThirtyMinutes, Timeframe.ThirtyMinutes.ToPeriodSize());
        Assert.Equal(PeriodSize.OneHour, Timeframe.OneHour.ToPeriodSize());
        Assert.Equal(PeriodSize.FourHours, Timeframe.FourHour.ToPeriodSize());
        Assert.Equal(PeriodSize.Day, Timeframe.Daily.ToPeriodSize());
        Assert.Equal(PeriodSize.Week, Timeframe.Weekly.ToPeriodSize());
        Assert.Equal(PeriodSize.Month, Timeframe.Monthly.ToPeriodSize());
    }

    [Fact]
    public void TestToPeriodSizeThrowsException()
    {
        Assert.Throws<ArgumentException>(() => ((Timeframe)999).ToPeriodSize()); // invalid value
    }
}