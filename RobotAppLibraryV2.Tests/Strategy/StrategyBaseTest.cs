using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.Enum;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Strategy;
using RobotAppLibraryV2.Tests.Candlelist;
using RobotAppLibraryV2.Tests.Factory;
using RobotAppLibraryV2.Tests.Strategy.ImplementationTests;
using Serilog;

namespace RobotAppLibraryV2.Tests.Strategy;

public class StrategyBaseTest
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly Mock<StrategyBase> _fakeStrategyTest;
    private readonly Mock<ILogger> _loggerMock = new();


    public StrategyBaseTest()
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
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100));

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

        // var listPositions = new List<Position>();
        //
        // var p1 = new Position()
        //     .SetSymbol("EURUSD")
        //     .SetStrategyId("StrategyBaseProxy-0.0.1-EURUSD-FifteenMinutes");
        //
        // var p2 = new Position()
        //     .SetSymbol("test")
        //     .SetStrategyId("test");
        //
        // var p3 = new Position()
        //     .SetSymbol("EURUSD")
        //     .SetStrategyId("StrategyBaseProxy-0.0.1-EURUSD-FifteenMinutes");
        //
        // listPositions.AddRange(new[] { p1, p2, p3 });
        //
        _apiHandlerMock.Setup(x => x.GetCurrentTradesAsync()).ReturnsAsync(new List<Position>());

        _fakeStrategyTest = new Mock<StrategyBase>(MockBehavior.Strict, new FakeStrategyBaseTest(), "EURUSD",
            Timeframe.FifteenMinutes,
            Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object);
    }

    #region Factory

    [Fact]
    public void Test_Factory_strategy()
    {
        // act
        var strategy = StrategyFactory.GenerateStrategy(new FakeStrategy(), "test", Timeframe.OneMinute,
            Timeframe.FifteenMinutes, _apiHandlerMock.Object, _loggerMock.Object);

        // assert
        strategy.Should().NotBeNull();
        strategy.Symbol.Should().Be("test");
        strategy.Timeframe.Should().Be(Timeframe.OneMinute);
        strategy.Timeframe2.Should().Be(Timeframe.FifteenMinutes);
    }

    #endregion

    #region Init

    [Fact]
    public void Test_init()
    {
        // arrange and act
        var strategyBase = _fakeStrategyTest.Object;

        // Assert
        strategyBase.Id.Should().NotBeEmpty();
        strategyBase.Symbol.Should().Be("EURUSD");
        strategyBase.Timeframe.Should().Be(Timeframe.FifteenMinutes);
        strategyBase.Timeframe2.Should().Be(Timeframe.OneHour);
        strategyBase.CanRun.Should().BeTrue();
    }

    #endregion

    #region CloseStrategy

    [Fact]
    public async void Test_CloseStrategy_Manual()
    {
        // Arrange 
        var listPositions = new List<Position>();

        var p1 = new Position()
            .SetSymbol("EURUSD")
            .SetStrategyId("FakeStrategyBaseTest-0.0.1-EURUSD-FifteenMinutes");

        var p2 = new Position()
            .SetSymbol("test")
            .SetStrategyId("test");

        var p3 = new Position()
            .SetSymbol("EURUSD")
            .SetStrategyId("FakeStrategyBaseTest-0.0.1-EURUSD-FifteenMinutes");

        listPositions.AddRange(new[] { p1, p2, p3 });

        _apiHandlerMock.Setup(x => x.GetCurrentTradesAsync()).ReturnsAsync(listPositions);

        var caller = false;

        _fakeStrategyTest.Object.StrategyClosed += (sender, args) => caller = true;

        _apiHandlerMock.Setup(x => x.GetTickPriceAsync(_fakeStrategyTest.Object.Symbol))
            .ReturnsAsync(new Tick(1, 1, DateTime.Now, "EURUSD"));

        // Act

        _fakeStrategyTest.Object.CloseStrategy(StrategyReasonClosed.Api);

        // Assert

        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Exactly(2));
        _apiHandlerMock.Verify(x => x.UnsubscribePrice(_fakeStrategyTest.Object.Symbol), Times.AtLeastOnce);
        caller.Should().BeTrue();
    }


    [Fact]
    public async void Test_CloseStrategy_By_Api_Disconnection()
    {
        // Arrange 
        var listPositions = new List<Position>();

        var p1 = new Position()
            .SetSymbol("EURUSD")
            .SetStrategyId("FakeStrategyTest-1-EURUSD-FifteenMinutes");

        var p2 = new Position()
            .SetSymbol("test")
            .SetStrategyId("test");

        var p3 = new Position()
            .SetSymbol("EURUSD")
            .SetStrategyId("FakeStrategyTest-1-EURUSD-FifteenMinutes");

        listPositions.AddRange(new[] { p1, p2, p3 });

        _apiHandlerMock.Setup(x => x.GetCurrentTradesAsync()).ReturnsAsync(listPositions);

        var caller = false;

        _fakeStrategyTest.Object.StrategyClosed += (sender, args) => caller = true;

        _apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick(1, 1, DateTime.Now, "EURUSD"));


        // Act

        _apiHandlerMock.Raise(x => x.Disconnected += null, this, EventArgs.Empty);

        // Assert

        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
        _apiHandlerMock.Verify(x => x.UnsubscribePrice(_fakeStrategyTest.Object.Symbol), Times.AtLeastOnce);
        caller.Should().BeTrue();
    }

    #endregion

    #region TickEvent

    #endregion
}