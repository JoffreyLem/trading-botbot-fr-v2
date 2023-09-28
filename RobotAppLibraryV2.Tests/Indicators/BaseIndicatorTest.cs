using FluentAssertions;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;

namespace RobotAppLibraryV2.Tests.Indicators;

public class IndicatorTest
{
    [Fact]
    public void Test_SarIndicator()
    {
        // Arrange
        var sarIndicaor = new SarIndicator();

        var candleList = TestUtils.GenerateCandle(TimeSpan.FromMinutes(Timeframe.FiveMinutes.GetMinuteFromTimeframe()));

        // Act
        sarIndicaor.UpdateIndicator(candleList);

        // Assert 
        sarIndicaor.First().Date.Should().Be(candleList.First().Date);
        sarIndicaor.Last().Date.Should().Be(candleList.Last().Date);
    }
}