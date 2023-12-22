using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.CandleList;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.MoneyManagement;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Result;
using RobotAppLibraryV2.Strategy;
using Serilog;

namespace RobotAppLibraryV2.Tests.Strategy;

public class StrategyTest
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<ICandleList> candleListMock = new();

    private readonly List<Candle> fakeHistory = TestUtils.GenerateCandle(TimeSpan.FromMinutes(5), 100);
    private readonly Mock<IMoneyManagement> moneyManagementMock = new();
    private readonly Mock<IPositionHandler> positionHandlerMock = new();

    private readonly StrategyBase strategyBase;

    private readonly Mock<StrategyImplementationBase> strategyImplementationBaseMock = new();

    private readonly Mock<IStrategyResult> strategyResultMock = new();
    private readonly Mock<IStrategyServiceFactory> strategyServiceFactoryMock = new();

    public StrategyTest()
    {
        _loggerMock.Setup(x => x.ForContext<StrategyBase>())
            .Returns(_loggerMock.Object);
        _loggerMock.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(_loggerMock.Object);

        strategyServiceFactoryMock.Setup(x =>
                x.GetMoneyManagement(It.IsAny<IApiHandler>(), It.IsAny<ILogger>(), It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(moneyManagementMock.Object);

        strategyServiceFactoryMock.Setup(x =>
                x.GetHistory(It.IsAny<ILogger>(), It.IsAny<IApiHandler>(), It.IsAny<string>(), It.IsAny<Timeframe>()))
            .Returns(candleListMock.Object);

        strategyServiceFactoryMock.Setup(x => x.GetStrategyResultService(It.IsAny<IApiHandler>(), It.IsAny<string>()))
            .Returns(strategyResultMock.Object);

        strategyServiceFactoryMock.Setup(x =>
                x.GetPositionHandler(It.IsAny<ILogger>(), It.IsAny<IApiHandler>(), It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(positionHandlerMock.Object);


        for (var i = 0; i < fakeHistory.Count; i++)
        {
            var capture = i;
            candleListMock.Setup(m => m[capture]).Returns(fakeHistory[capture]);
        }

        candleListMock.SetupGet(cl => cl.Count).Returns(fakeHistory.Count);

        candleListMock.SetupGet(x => x.LastPrice).Returns(new Tick());

        strategyBase = new StrategyBase(strategyImplementationBaseMock.Object, "EURUSD",
            Timeframe.FifteenMinutes, Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object,
            strategyServiceFactoryMock.Object);
    }

    #region Treshold

    [Theory]
    [InlineData(EventTreshold.Drowdown)]
    [InlineData(EventTreshold.Profitfactor)]
    [InlineData(EventTreshold.LooseStreak)]
    [InlineData(EventTreshold.ProfitTreshHold)]
    public void Test_MoneyManagement_Treshold(EventTreshold eventTreshold)
    {
        // Arrange
        var caller = false;

        strategyBase.StrategyDisabledEvent += (sender, treshold) =>
        {
            caller = true;
            treshold.EventField.Should().NotBe(StrategyReasonDisabled.User);
        };

        strategyResultMock.Raise(x => x.ResultTresholdEvent += null, this, eventTreshold);

        caller.Should().BeTrue();
    }

    #endregion

    #region Init

    [Fact]
    public void Test_Init()
    {
        _apiHandlerMock.Verify(x => x.SubscribePrice("EURUSD"), Times.Exactly(1));
        strategyImplementationBaseMock.Object.History.Should().BeSameAs(candleListMock.Object);
        strategyImplementationBaseMock.Object.LastPrice.Should().Be(candleListMock.Object.LastPrice);
        strategyImplementationBaseMock.Object.LastCandle.Should().Be(fakeHistory[^2]);
        strategyImplementationBaseMock.Object.Logger.Should().BeSameAs(_loggerMock.Object);
        positionHandlerMock.VerifySet(m => m.DefaultSl = 50, Times.Once());
        positionHandlerMock.VerifySet(m => m.DefaultTp = 50, Times.Once());
    }

    [Fact]
    public void Test_ParameterUpdate_RunOnTick()
    {
        strategyImplementationBaseMock.Object.RunOnTick = true;
        strategyBase.RunOnTick.Should().Be(true);

        strategyImplementationBaseMock.Object.RunOnTick = false;
        strategyBase.RunOnTick.Should().Be(false);
    }

    [Fact]
    public void Test_ParameterUpdate_UpdateOnTick()
    {
        strategyImplementationBaseMock.Object.UpdateOnTick = true;
        strategyBase.UpdateOnTick.Should().Be(true);

        strategyImplementationBaseMock.Object.UpdateOnTick = false;
        strategyBase.UpdateOnTick.Should().Be(false);
    }

    [Fact]
    public void Test_ParameterUpdate_CloseOnTick()
    {
        strategyImplementationBaseMock.Object.CloseOnTick = true;
        strategyBase.CloseOnTick.Should().Be(true);

        strategyImplementationBaseMock.Object.CloseOnTick = false;
        strategyBase.CloseOnTick.Should().Be(false);
    }

    [Fact]
    public void Test_ParameterUpdate_CanRun()
    {
        strategyImplementationBaseMock.Object.CanRun = true;
        strategyBase.CanRun.Should().Be(true);

        strategyImplementationBaseMock.Object.CanRun = false;
        strategyBase.CanRun.Should().Be(false);
    }

    [Fact]
    public void Test_ParameterUpdate_DefaultSl()
    {
        strategyBase.DefaultStopLoss = 1;
        positionHandlerMock.VerifySet(m => m.DefaultSl = 1, Times.Once());
    }

    [Fact]
    public void Test_ParameterUpdate_DefaultTp()
    {
        strategyBase.DefaultTakeProfit = 1;
        positionHandlerMock.VerifySet(m => m.DefaultTp = 1, Times.Once());
    }

    #endregion

    #region Tick Region

    [Fact]
    public void Test_OnTickRun_Common_Event()
    {
        var callerTick = false;
        var callerCandle = false;
        var tickToSend = new Tick();
        strategyBase.TickEvent += (sender, tick) =>
        {
            callerTick = true;
            tick.Should().Be(tickToSend);
        };
        strategyBase.CandleEvent += (sender, candle) =>
        {
            callerCandle = true;
            candle.Should().Be(fakeHistory.LastOrDefault());
        };

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, tickToSend);

        // Assert
        callerTick.Should().BeTrue();
        callerCandle.Should().BeFalse();
    }

    [Fact]
    public void Test_OnNewTick_Run()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(false);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.Run(), Times.Exactly(1));
    }

    [Theory]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void Test_OnNewTick_NoRun(bool canrun, bool runOnTick, bool positionInProgress)
    {
        // Arrange
        strategyBase.CanRun = canrun;
        strategyBase.RunOnTick = runOnTick;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(positionInProgress);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.Run(), Times.Exactly(0));
    }

    [Fact]
    public void Test_OnNewTick_Run_PositionInProgress_Update()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.UpdateOnTick = true;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldUpdatePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldUpdatePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void Test_OnNewTick_Run_PositionInProgress_Update_Should_Update_False()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.UpdateOnTick = true;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldUpdatePosition(It.IsAny<Position>()))
            .Returns(false);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldUpdatePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public void Test_OnNewTick_Run_PositionInProgress_Update_TickRun_False()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.UpdateOnTick = false;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldUpdatePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldUpdatePosition(It.IsAny<Position>()), Times.Exactly(0));
        positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public void Test_OnNewTick_Run_PositionInProgress_Close()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.CloseOnTick = true;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldClosePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldClosePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void Test_OnNewTick_Run_PositionInProgress_Close_Should_Close_False()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.CloseOnTick = true;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldClosePosition(It.IsAny<Position>()))
            .Returns(false);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldClosePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public void Test_OnNewTick_Run_PositionInProgress_Update_Close_False()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.CloseOnTick = false;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldClosePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldClosePosition(It.IsAny<Position>()), Times.Exactly(0));
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Never);
    }

    #endregion

    #region Candle Event

    [Fact]
    public void Test_OnCandleEvent_Run()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = false;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(false);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.Run(), Times.Exactly(1));
    }

    [Theory]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    public void Test_OnNewCandle_NoRun(bool canrun, bool runOnTick, bool positionInProgress)
    {
        // Arrange
        strategyBase.CanRun = canrun;
        strategyBase.RunOnTick = runOnTick;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(positionInProgress);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.Run(), Times.Exactly(0));
    }

    [Fact]
    public void Test_OnNewCandle_Run_PositionInProgress_Update()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.UpdateOnTick = false;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldUpdatePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldUpdatePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void Test_OnNewCandle_Run_PositionInProgress_Update_Should_Update_False()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.UpdateOnTick = false;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldUpdatePosition(It.IsAny<Position>()))
            .Returns(false);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldUpdatePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public void Test_OnNewCandle_Run_PositionInProgress_Update_TickRun_true()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.UpdateOnTick = true;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldUpdatePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldUpdatePosition(It.IsAny<Position>()), Times.Exactly(0));
        positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public void Test_OnNewCandle_Run_PositionInProgress_Close()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.CloseOnTick = false;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldClosePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldClosePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void Test_OnNewCandle_Run_PositionInProgress_Close_Should_Close_False()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.CloseOnTick = false;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldClosePosition(It.IsAny<Position>()))
            .Returns(false);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldClosePosition(It.IsAny<Position>()), Times.Exactly(1));
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public void Test_OnNewCandle_Run_PositionInProgress_Update_Close_True()
    {
        // Arrange
        strategyBase.CanRun = true;
        strategyBase.RunOnTick = true;
        strategyBase.CloseOnTick = true;
        positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImplementationBaseMock.Setup(x => x.ShouldClosePosition(It.IsAny<Position>()))
            .Returns(true);

        // Act 
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert
        strategyImplementationBaseMock.Verify(x => x.ShouldClosePosition(It.IsAny<Position>()), Times.Exactly(0));
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Never);
    }

    #endregion

    #region Close Strategy

    [Theory]
    [InlineData(StrategyReasonDisabled.User)]
    [InlineData(StrategyReasonDisabled.Api)]
    [InlineData(StrategyReasonDisabled.Error)]
    [InlineData(StrategyReasonDisabled.Treshold)]
    public void Test_EventClose(StrategyReasonDisabled strategyReasonClosed)
    {
        var caller = false;

        strategyBase.StrategyDisabledEvent += (sender, closed) =>
        {
            caller = true;
            closed.Should().Be(strategyReasonClosed);
        };


        strategyBase.DisableStrategy(strategyReasonClosed);

        caller.Should().BeTrue();
        strategyBase.CanRun.Should().BeFalse();
        strategyBase.StrategyDisabled.Should().BeTrue();
    }

    [Fact]
    public void Test_CloseStrategy_User_Reason()
    {
        // Arrange
        _apiHandlerMock.Setup(x => x.GetCurrentTradeAsync(It.IsAny<string>()))
            .ReturnsAsync(new Position());


        // Act
        strategyBase.DisableStrategy(StrategyReasonDisabled.User);

        // Assert
        _apiHandlerMock.Verify(x => x.GetCurrentTradeAsync(It.IsAny<string>()), Times.Once);
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Exactly(1));
        _apiHandlerMock.Verify(x => x.UnsubscribePrice(It.IsAny<string>()), Times.Once);
        strategyBase.StrategyDisabled.Should().BeTrue();
    }

    [Fact]
    public void Test_CloseStrategy_Api_Reason()
    {
        // Act and Arrange
        strategyBase.DisableStrategy(StrategyReasonDisabled.Api);

        // Assert
        _apiHandlerMock.Verify(x => x.GetCurrentTradeAsync(It.IsAny<string>()), Times.Never);
        positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Never);
        _apiHandlerMock.Verify(x => x.UnsubscribePrice(It.IsAny<string>()), Times.Once);
        strategyBase.StrategyDisabled.Should().BeTrue();
    }

    #endregion

    #region Open position

    [Fact]
    public void Test_OpenPosition_StrategyVolume_define()
    {
        // Arrange
        var lastPrice = new Tick
        {
            Bid = 1,
            Ask = 2
        };

        candleListMock.SetupGet(x => x.LastPrice).Returns(lastPrice);

        moneyManagementMock.Setup(x =>
                x.CalculatePositionSize(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<double>()))
            .Returns(10);

        // act

        strategyImplementationBaseMock.Object.OpenPositionAction.Invoke(TypeOperation.Buy, 1, 1, 1, 1, 1);

        // Assert
        positionHandlerMock.Verify(x => x.OpenPositionAsync(
            It.Is<string>(x => x == "EURUSD"),
            It.Is<TypeOperation>(x => x == TypeOperation.Buy),
            It.Is<double>(x => x == 1),
            It.Is<decimal>(x => x == 1),
            It.Is<decimal>(x => x == 1),
            It.Is<long?>(x => x == 1)
        ), Times.Once);
    }

    [Fact]
    public void Test_OpenPosition_NoSl_StrategyVolume_define()
    {
        // Arrange
        var lastPrice = new Tick
        {
            Bid = 1,
            Ask = 2
        };

        candleListMock.SetupGet(x => x.LastPrice).Returns(lastPrice);

        moneyManagementMock.SetupGet(x => x.SymbolInfo).Returns(new SymbolInfo
        {
            LotMin = 20
        });

        moneyManagementMock.Setup(x =>
                x.CalculatePositionSize(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<double>()))
            .Returns(10);

        // act

        strategyImplementationBaseMock.Object.OpenPositionAction.Invoke(TypeOperation.Buy, 0, 1, 1, 1, 1);

        // Assert
        positionHandlerMock.Verify(x => x.OpenPositionAsync(
            It.Is<string>(x => x == "EURUSD"),
            It.Is<TypeOperation>(x => x == TypeOperation.Buy),
            It.Is<double>(x => x == 1),
            It.Is<decimal>(x => x == 0),
            It.Is<decimal>(x => x == 1),
            It.Is<long?>(x => x == 1)
        ), Times.Once);
    }

    [Fact]
    public void Test_OpenPosition_MoneyManagement_define()
    {
        // Arrange
        var lastPrice = new Tick
        {
            Bid = 1,
            Ask = 2
        };

        candleListMock.SetupGet(x => x.LastPrice).Returns(lastPrice);

        moneyManagementMock.Setup(x =>
                x.CalculatePositionSize(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<double>()))
            .Returns(10);

        // act

        strategyImplementationBaseMock.Object.OpenPositionAction.Invoke(TypeOperation.Buy, 1, 1, 1, null, 1);

        // Assert
        positionHandlerMock.Verify(x => x.OpenPositionAsync(
            It.Is<string>(x => x == "EURUSD"),
            It.Is<TypeOperation>(x => x == TypeOperation.Buy),
            It.Is<double>(x => x == 10),
            It.Is<decimal>(x => x == 1),
            It.Is<decimal>(x => x == 1),
            It.Is<long?>(x => x == 1)
        ), Times.Once);
    }

    [Fact]
    public void Test_OpenPosition_NoSl_MoneyManagement_define()
    {
        // Arrange
        var lastPrice = new Tick
        {
            Bid = 1,
            Ask = 2
        };

        candleListMock.SetupGet(x => x.LastPrice).Returns(lastPrice);

        moneyManagementMock.SetupGet(x => x.SymbolInfo).Returns(new SymbolInfo
        {
            LotMin = 20
        });

        moneyManagementMock.Setup(x =>
                x.CalculatePositionSize(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<double>()))
            .Returns(10);

        // act

        strategyImplementationBaseMock.Object.OpenPositionAction.Invoke(TypeOperation.Buy, 0, 1, 1, null, 1);

        // Assert
        positionHandlerMock.Verify(x => x.OpenPositionAsync(
            It.Is<string>(x => x == "EURUSD"),
            It.Is<TypeOperation>(x => x == TypeOperation.Buy),
            It.Is<double>(x => x == 20),
            It.Is<decimal>(x => x == 0),
            It.Is<decimal>(x => x == 1),
            It.Is<long?>(x => x == 1)
        ), Times.Once);
    }

    #endregion
}