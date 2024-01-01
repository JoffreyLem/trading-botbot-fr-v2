using FluentAssertions;
using RobotAppLibraryV2.BackTest;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Tests.Utils.Utils;

public class CandleHelperTest
{
    [Fact]
    public void DecomposeCandlestick_ShouldReturnCorrectNumberOfTicks()
    {
        // Arrange
        var candle = new Candle { Date = new DateTime(2023, 1, 1), Open = 100, High = 110, Low = 90, Close = 105 };
        var timeframe = Timeframe.FifteenMinutes;
        var askBidSpread = 0.5m;
        var symbol = "TEST";

        // Act
        var ticks = CandleHelper.DecomposeCandlestick(candle, timeframe, askBidSpread, new SymbolInfo
        {
            Symbol = "eurusd"
        });

        // Assert
        ticks.Should().HaveCount(4);
        ticks[0].Bid.Should().Be(100);
        ticks[1].Bid.Should().Be(110);
        ticks[2].Bid.Should().Be(90);
        ticks[3].Bid.Should().Be(105);
    }
}