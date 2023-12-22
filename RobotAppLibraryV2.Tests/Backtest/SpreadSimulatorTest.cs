using FluentAssertions;
using RobotAppLibraryV2.BackTest;

namespace RobotAppLibraryV2.Tests.Backtest;

public class SpreadSimulatorTest
{
    [Fact]
    public void GenerateSpread_ReturnsValueWithinRange()
    {
        // Arrange
        var minSpread = 0.5m;
        var maxSpread = 3.0m;
        var simulator = new SpreadSimulator(minSpread, maxSpread);

        // Act
        var result = simulator.GenerateSpread();

        // Assert
        result.Should().BeGreaterOrEqualTo(minSpread).And.BeLessOrEqualTo(maxSpread);
    }

    [Fact]
    public void GenerateSpread_ReturnsRoundedValue()
    {
        // Arrange
        var minSpread = 0.0m;
        var maxSpread = 10.0m;
        var simulator = new SpreadSimulator(minSpread, maxSpread);

        // Act
        var result = simulator.GenerateSpread();

        // Assert
        var remainder = result * 10 % 1;
        remainder.Should().Be(0);
    }

    [Fact]
    public void GenerateSpread_MultipleCalls_ReturnDifferentValues()
    {
        // This test might not always pass because there's a chance the random value can be the same twice.
        // However, it's a good sanity check. If it fails frequently, there might be an issue.

        // Arrange
        var minSpread = 0.0m;
        var maxSpread = 10.0m;
        var simulator = new SpreadSimulator(minSpread, maxSpread);

        // Act
        var result1 = simulator.GenerateSpread();
        var result2 = simulator.GenerateSpread();

        // Assert
        result1.Should().NotBe(result2);
    }
}