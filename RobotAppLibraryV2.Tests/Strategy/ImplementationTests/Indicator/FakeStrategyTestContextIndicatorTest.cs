using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Strategy;
using RobotAppLibraryV2.Tests.Candlelist;
using RobotAppLibraryV2.Utils;
using Serilog;

namespace RobotAppLibraryV2.Tests.Strategy.ImplementationTests.Indicator;

public class FakeStrategyTestContextIndicatorTest
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly FakeStrategyContextIndicator _fakeStrategyTest = new();
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly StrategyBase _strategyBase;

    public FakeStrategyTestContextIndicatorTest()
    {
        _loggerMock.Setup(x => x.ForContext<RobotAppLibraryV2.MoneyManagement.MoneyManagement>())
            .Returns(_loggerMock.Object);
        _loggerMock.Setup(x => x.ForContext<CandleList.CandleList>())
            .Returns(_loggerMock.Object);
        _loggerMock.Setup(x => x.ForContext<PositionHandler>())
            .Returns(_loggerMock.Object);
        _loggerMock.Setup(x => x.ForContext<StrategyBase>())
            .Returns(_loggerMock.Object);
        _loggerMock.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<string>(), false))
            .Returns(_loggerMock.Object);

        // ApiHandlerInit

        _apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new AccountBalance
        {
            Balance = 10000
        });

        _apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.09755 });

        var candleList = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);

        candleList.Last().Date = DateTime.Now;

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candleList);

        _apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo()
                .WithLeverage(3.33)
                .WithSymbol("EURUSD")
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCategory(Category.Forex)
            );

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(CandleListTest.GetTradeHoursMock);

        _apiHandlerMock.Setup(x => x.GetCurrentTradesAsync()).ReturnsAsync(new List<Position>());

        _strategyBase = new StrategyBase(_fakeStrategyTest, "EURUSD", Timeframe.FifteenMinutes,
            Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object);
    }


    [Fact]
    public void Test_Count_Indicator()
    {
        // Arrange and act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));

        // Assert 
        _fakeStrategyTest.SarIndicator.Count.Should().Be(101);
    }
}