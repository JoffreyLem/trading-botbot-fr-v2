using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Strategy;
using RobotAppLibraryV2.Tests.Candlelist;
using RobotAppLibraryV2.Utils;
using Serilog;

namespace RobotAppLibraryV2.Tests.Strategy.ImplementationTests.Positions;

public class FakeStrategyTestContextFalseTest
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly FakeStrategyContextFalse _fakeStrategyTest = new();
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly StrategyBase _strategyBase;

    public FakeStrategyTestContextFalseTest()
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
                .WithCategory(Category.Forex));

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(CandleListTest.GetTradeHoursMock);

        _apiHandlerMock.Setup(x => x.GetCurrentTradesAsync()).ReturnsAsync(new List<Position>());

        _strategyBase = new StrategyBase(_fakeStrategyTest, "EURUSD", Timeframe.FifteenMinutes,
            Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object);
    }

    #region tick_OpenPosition

    [Fact]
    public void Test_OpenPosition_Tick()
    {
        // Arrange
        _strategyBase.RunOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Once);
    }


    [Fact]
    public async void Test_OpenPosition_Tick_PositionAlready_inProgress()
    {
        // Arrange
        _strategyBase.RunOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));
        _apiHandlerMock.Reset();
        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public void Test_OpenPosition_RunOnTick_False()
    {
        // Arrange
        _strategyBase.RunOnTick = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public void Test_OpenPosition_CanRun_False()
    {
        // Arrange
        _strategyBase.RunOnTick = true;
        _strategyBase.CanRun = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public void Test_OpenPosition_Tick_throw_exception()
    {
        // Arrange
        _strategyBase.RunOnTick = true;
        var caller = false;

        _strategyBase.StrategyClosed += (sender, args) => caller = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>())).ThrowsAsync(new Exception());

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Assert
        caller.Should().BeFalse();
    }

    #endregion

    #region Tick_UpdatePosition

    [Fact]
    public void Test_UpdatePosition_Tick()
    {
        // Arrange
        _strategyBase.RunOnTick = true;
        _strategyBase.UpdateOnTick = true;


        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public void Test_UpdatePosition_Tick_NoUpdateOnTick()
    {
        // Arrange
        _strategyBase.RunOnTick = true;
        _strategyBase.UpdateOnTick = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }

    #endregion

    #region tick_closePosition

    [Fact]
    public void Test_ClosePosition_Tick()
    {
        // Arrange
        _strategyBase.RunOnTick = true;
        _strategyBase.CloseOnTick = true;


        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public void Test_ClosePosition_Tick_NoUpdateOnTick()
    {
        // Arrange
        _strategyBase.RunOnTick = true;
        _strategyBase.UpdateOnTick = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(1), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, new Tick(1, 1, DateTime.Now.AddSeconds(2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }

    #endregion


    #region Candle Open position

    [Fact]
    public void Test_OpenPosition_Candle()
    {
        // Arrange
        _strategyBase.RunOnTick = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void Test_OpenPosition_Candle_runOnTick_true()
    {
        // Arrange
        _strategyBase.RunOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });

        // Act
        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async void Test_OpenPosition_Candle_PositionAlready_inProgress()
    {
        // Arrange
        _strategyBase.RunOnTick = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });


        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));
        _apiHandlerMock.Reset();
        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe() * 2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async void Test_OpenPosition_Candle_CanRun_False()
    {
        // Arrange
        _strategyBase.RunOnTick = false;
        _strategyBase.CanRun = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });


        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));
        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe() * 2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.OpenPositionAsync(It.IsAny<Position>()), Times.Never);
    }

    #endregion

    #region Candle Update Position

    [Fact]
    public async void Test_UpdatePosition_Candle_UpdateOnTick_false()
    {
        // Arrange
        _strategyBase.UpdateOnTick = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });


        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe() * 2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public async void Test_UpdatePosition_Candle_UpdateOnTick_true()
    {
        // Arrange
        _strategyBase.UpdateOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });


        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe() * 2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }

    #endregion

    #region Candle close on tick

    [Fact]
    public async void Test_ClosePosition_Candle_CloseOnTick_false()
    {
        // Arrange
        _strategyBase.CloseOnTick = false;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });


        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe() * 2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public async void Test_ClosePosition_Candle_CloseOnTick_true()
    {
        // Arrange
        _strategyBase.CloseOnTick = true;

        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .Callback<Position>(position =>
            {
                _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);
            });


        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe()), "EURUSD"));

        // Act

        _apiHandlerMock.Raise(x => x.TickEvent += null, this,
            new Tick(1, 1, DateTime.Now.AddMinutes(Timeframe.FifteenMinutes.GetMinuteFromTimeframe() * 2), "EURUSD"));

        // Assert
        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }

    #endregion
}