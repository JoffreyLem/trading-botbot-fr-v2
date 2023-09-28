using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Strategy;
using RobotAppLibraryV2.Tests.Candlelist;
using Serilog;

namespace RobotAppLibraryV2.Tests.Strategy.ImplementationTests.Volume;

public class FakeStrategyTestContextVolume
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly FakeStrategyTestContextNoSlNoVolume _fakeStrategyTestContextNoSlNoVolume = new();
    private readonly FakeStrategyTestContextNoSlVolume _fakeStrategyTestContextNoSlVolume = new();
    private readonly FakeStrategyTestContextSlNoVolume _fakeStrategyTestContextSlNoVolume = new();
    private readonly FakeStrategyTestContextSlVolume _fakeStrategyTestContextSlVolume = new();
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly StrategyBase _strategyBaseContextNoSlNoVolume;
    private readonly StrategyBase _strategyBaseContextNoSlVolume;
    private readonly StrategyBase _strategyBaseContextSlNoVolume;
    private readonly StrategyBase _strategyBaseContextSlVolume;


    public FakeStrategyTestContextVolume()
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
            Balance = 10000,
            MarginFree = 10000,
            Equity = 10000
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
                .WithLotMin(0.01)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCategory(Category.Forex));

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(CandleListTest.GetTradeHoursMock);

        _apiHandlerMock.Setup(x => x.GetCurrentTradesAsync()).ReturnsAsync(new List<Position>());

        _strategyBaseContextNoSlNoVolume = new StrategyBase(_fakeStrategyTestContextNoSlNoVolume, "EURUSD",
            Timeframe.FifteenMinutes,
            Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object);

        _strategyBaseContextNoSlVolume = new StrategyBase(_fakeStrategyTestContextNoSlVolume, "EURUSD",
            Timeframe.FifteenMinutes,
            Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object);

        _strategyBaseContextSlNoVolume = new StrategyBase(_fakeStrategyTestContextSlNoVolume, "EURUSD",
            Timeframe.FifteenMinutes,
            Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object);

        _strategyBaseContextSlVolume = new StrategyBase(_fakeStrategyTestContextSlVolume, "EURUSD",
            Timeframe.FifteenMinutes,
            Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Test_OpenPosition_Tick_checkVolume_no_sl_no_volume()
    {
        // Arrange
        _strategyBaseContextNoSlNoVolume.RunOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.Is<Position>(x => x.Volume == 0.01)), Times.Once);
    }

    [Fact]
    public void Test_OpenPosition_Tick_checkVolume_no_sl_volume()
    {
        // Arrange
        _strategyBaseContextNoSlVolume.RunOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.Is<Position>(x => x.Volume == 0.10)), Times.Once);
    }

    [Fact]
    public void Test_OpenPosition_Tick_checkVolume_sl_volume()
    {
        // Arrange
        _strategyBaseContextSlVolume.RunOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.Is<Position>(x => x.Volume == 0.20)), Times.Once);
    }

    [Fact]
    public void Test_OpenPosition_Tick_checkVolume_sl_no_volume()
    {
        // Arrange
        _strategyBaseContextSlNoVolume.RunOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.Is<Position>(x => x.Volume == 0.02)), Times.Once);
    }
}