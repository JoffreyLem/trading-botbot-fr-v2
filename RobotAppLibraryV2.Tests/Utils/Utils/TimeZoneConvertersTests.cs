using FluentAssertions;
using RobotAppLibraryV2.Utils;

namespace RobotAppLibraryV2.Tests.Utils.Utils;

public class TimeZoneConvertersTests
{
    [Fact]
    public void ConvertCetCestToUtc_Should_ConvertCorrectly()
    {
        // Arrange
        var cetTime = new DateTime(2023, 4, 1, 12, 0, 0); // Un temps en CET
        var expectedUtcTime = new DateTime(2023, 4, 1, 10, 0, 0); // L'heure UTC attendue

        // Act
        var result = TimeZoneConverter.ConvertCetCestToUtc(cetTime);

        // Assert
        result.Should().BeCloseTo(expectedUtcTime, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ConvertMillisecondsToUtc_Should_ConvertCorrectly()
    {
        // Arrange
        var milliseconds = 1609459200000; // Timestamp Unix en millisecondes
        var expectedDateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = TimeZoneConverter.ConvertMillisecondsToUtc(milliseconds);

        // Assert
        result.Should().BeCloseTo(expectedDateTime, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void ConvertMidnightCetCestMillisecondsToUtc_Should_ConvertCorrectly()
    {
        // Arrange
        var milliseconds = 1609459200000; // Timestamp Unix pour une date en CET/CEST
        var expectedDateTime =
            new DateTime(2020, 12, 31, 23, 0, 0, DateTimeKind.Utc); // Minuit CET/CEST converti en UTC

        // Act
        var result = TimeZoneConverter.ConvertMidnightCetCestMillisecondsToUtc(milliseconds);

        // Assert
        result.Should().BeCloseTo(expectedDateTime, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ConvertMidnightCetCestMillisecondsToUtcOffset_Should_CalculateOffsetCorrectly()
    {
        // Arrange
        var milliseconds = 1609459200000; // Timestamp Unix pour une date en CET/CEST
        var expectedOffset = TimeSpan.FromHours(23); // L'offset attendu pour CET/CEST à minuit

        // Act
        var offset = TimeZoneConverter.ConvertMidnightCetCestMillisecondsToUtcOffset(milliseconds);

        // Assert
        offset.Should().Be(expectedOffset);
    }
    
    [Fact]
    public void ConvertMidnightCetCestMillisecondsToUtcOffset_Should_CalculateOffsetCorrectly_2()
    {
        // Arrange
        var milliseconds = 79200000; // Timestamp Unix pour une date en CET/CEST
        var expectedOffset = TimeSpan.FromHours(21); // L'offset attendu pour CET/CEST à minuit

        // Act
        var offset = TimeZoneConverter.ConvertMidnightCetCestMillisecondsToUtcOffset(milliseconds);

        // Assert
        offset.Should().Be(expectedOffset);
    }
}