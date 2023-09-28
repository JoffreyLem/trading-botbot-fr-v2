using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Backtest;
using RobotAppLibraryV2.ApiHandler.Exception;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.Tests.Backtest;

public class ApiHandlerBacktestTest
{
    private const string positionRefId = "1";
    private readonly Mock<IApiHandler> _apiHandlerMock = new();

    private readonly BacktestApiHandler _backtestApiHandler;
    private readonly Mock<ILogger> _logger = new();
    private readonly DateTime dateRef = DateTime.Now.AddDays(-4);

    public ApiHandlerBacktestTest()
    {
        var spreadSimulator = new SpreadSimulator(1, 1);
        var accountBalance = new AccountBalance();

        var symbolInfo = new SymbolInfo()
            .WithLeverage(3.33)
            .WithTickSize(0.00001)
            .WithTickSize2(0.0001)
            .WithContractSize(100000)
            .WithCurrency1("EUR")
            .WithCurrency2("USD")
            .WithSymbol("EURUSD")
            .WithCategory(Category.Forex);

        _apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        _apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 1.10559m, Date = dateRef })
            .ReturnsAsync(new Tick { Bid = 1.10559m, Date = dateRef });

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(new List<Candle>
            {
                new()
                {
                    Open = 1.10539m,
                    High = 1.10529m,
                    Low = 1.10519m,
                    Close = 1.10509m,
                    Date = DateTime.Now.Date.AddDays(-2)
                },
                new()
                {
                    Open = 1.10599m,
                    High = 1.10589m,
                    Low = 1.10579m,
                    Close = 1.10569m,
                    Date = DateTime.Now.Date.AddDays(-1)
                }
            });

        _backtestApiHandler = new BacktestApiHandler(_apiHandlerMock.Object, spreadSimulator, _logger.Object,
            accountBalance, "EURUSD");
    }

    #region OpenPosition

    [Fact]
    public void Test_OpenPosition()
    {
        // Arrange
        var position = new Position().SetId(positionRefId);
        var caller = false;
        _backtestApiHandler.PositionOpenedEvent += (sender, position1) =>
        {
            caller = true;
            position1.StatusPosition.Should().Be(StatusPosition.Open);
            position1.OpenPrice.Should().Be(1.10559m);
            position1.Id.Should().Be("1");
            position1.DateOpen.Should().Be(dateRef);
        };

        // Act && Assert
        _backtestApiHandler.OpenPositionAsync(position);
        caller.Should().BeTrue();
    }

    #endregion


    private void mock_openPosition(string id, decimal? sl, decimal? tp, TypePosition typePosition)
    {
        var position = new Position().SetId(id).SetStopLoss(sl).SetTakeProfit(tp).SetSpread(1)
            .SetTypePosition(typePosition).SetVolume(0.5);
        _backtestApiHandler.OpenPositionAsync(position);
    }

    #region UpdatePosition

    [Fact]
    public void Test_UpdatePosition_NoPosition()
    {
        // Arrange
        var position = new Position();

        // Act
        var act = () => _backtestApiHandler.UpdatePositionAsync(1, position);

        // Asset
        act.Should().ThrowAsync<ApiHandlerException>();
    }

    [Fact]
    public void Test_UpdatePosition_NoPosition_NoMatchingId()
    {
        // Arrange
        mock_openPosition("1", null, null, TypePosition.Buy);
        var position = new Position().SetId("2");

        // Act
        var act = () => _backtestApiHandler.UpdatePositionAsync(1, position);

        // Asset
        act.Should().ThrowAsync<ApiHandlerException>();
    }

    [Fact]
    public void Test_UpdatePosition()
    {
        // Arrange
        mock_openPosition("1", 1, 1, TypePosition.Buy);
        var position = new Position().SetId("1").SetStopLoss(2).SetTakeProfit(2);
        var caller = false;
        _backtestApiHandler.PositionUpdatedEvent += (sender, position1) =>
        {
            caller = true;
            position1.StopLoss.Should().Be(2);
            position1.TakeProfit.Should().Be(2);
        };

        // Act && Assert
        _backtestApiHandler.UpdatePositionAsync(1, position);
        caller.Should().BeTrue();
    }

    #endregion

    #region ClosePosition

    [Fact]
    public void Test_ClosePosition_NoPosition()
    {
        // Arrange
        var position = new Position();

        // Act
        var act = () => _backtestApiHandler.ClosePositionAsync(1, position);

        // Asset
        act.Should().ThrowAsync<ApiHandlerException>();
    }

    [Fact]
    public void Test_ClosePosition_NoPosition_NoMatchingId()
    {
        // Arrange
        mock_openPosition("1", null, null, TypePosition.Buy);
        var position = new Position().SetId("2");

        // Act
        var act = () => _backtestApiHandler.ClosePositionAsync(1, position);

        // Asset
        act.Should().ThrowAsync<ApiHandlerException>();
    }

    [Fact]
    public void Test_ClosePosition()
    {
        // Arrange
        mock_openPosition("1", 1, 1, TypePosition.Buy);
        var position = new Position().SetId("1").SetStopLoss(2).SetTakeProfit(2);
        var caller = false;
        _backtestApiHandler.PositionClosedEvent += (sender, position1) =>
        {
            caller = true;
            position1.StatusPosition.Should().Be(StatusPosition.Close);
            position1.ClosePrice.Should().Be(1.10559M);
            position1.DateClose.Should().Be(dateRef);
        };

        // Act && Assert
        _backtestApiHandler.ClosePositionAsync(1, position);
        caller.Should().BeTrue();
    }

    #endregion

    #region Start

    [Fact]
    public async void Test_Flow_Tick()
    {
        // Arrange
        var ticks = new List<Tick>();
        _backtestApiHandler.TickEvent += (sender, tick) => ticks.Add(tick);

        // Act
        await _backtestApiHandler.StartAsync("EURUSD", Timeframe.OneMinute);
        var datesUniques = ticks.Select(o => o.Date).Distinct().Count();
        var totalDates = ticks.Count;
        // Assert
        datesUniques.Should().Be(totalDates);
    }

    [Fact]
    public async void Test_ClosePosition_Running_Sl_buy()
    {
        // Arrange
        mock_openPosition("1", 1.10539m, 1.10589m, TypePosition.Buy);

        var caller = false;
        _backtestApiHandler.PositionClosedEvent += (sender, position) =>
        {
            caller = true;
            position.ClosePrice.Should().Be(1.10539m);
            position.Profit.Should().BeNegative();
            position.DateClose.Should().BeMoreThan(position.DateOpen.TimeOfDay);
        };

        // Act
        await _backtestApiHandler.StartAsync("EURUSD", Timeframe.OneMinute);

        caller.Should().BeTrue();
    }

    [Fact]
    public async void Test_ClosePosition_Running_Tp_buy()
    {
        // Arrange
        mock_openPosition("1", 1.10239m, 1.10589m, TypePosition.Buy);

        var caller = false;
        _backtestApiHandler.PositionClosedEvent += (sender, position) =>
        {
            caller = true;
            position.ClosePrice.Should().Be(1.10589m);
            position.Profit.Should().BePositive();
            position.DateClose.Should().BeMoreThan(position.DateOpen.TimeOfDay);
        };

        // Act
        await _backtestApiHandler.StartAsync("EURUSD", Timeframe.OneMinute);

        caller.Should().BeTrue();
    }

    [Fact]
    public async void Test_ClosePosition_Running_Sl_Sell()
    {
        // Arrange
        mock_openPosition("1", 1.10589m, 1.10439m, TypePosition.Sell);

        var caller = false;
        _backtestApiHandler.PositionClosedEvent += (sender, position) =>
        {
            caller = true;
            position.ClosePrice.Should().Be(1.10589m);
            position.Profit.Should().BeNegative();
            position.DateClose.Should().BeMoreThan(position.DateOpen.TimeOfDay);
        };

        // Act
        await _backtestApiHandler.StartAsync("EURUSD", Timeframe.OneMinute);

        caller.Should().BeTrue();
    }

    [Fact]
    public async void Test_ClosePosition_Running_Tp_sell()
    {
        // Arrange
        mock_openPosition("1", 1.10989m, 1.10509m, TypePosition.Sell);

        var caller = false;
        _backtestApiHandler.PositionClosedEvent += (sender, position) =>
        {
            caller = true;
            position.ClosePrice.Should().Be(1.10509m);
            position.Profit.Should().BePositive();
            position.DateClose.Should().BeMoreThan(position.DateOpen.TimeOfDay);
        };

        // Act
        await _backtestApiHandler.StartAsync("EURUSD", Timeframe.OneMinute);

        caller.Should().BeTrue();
    }

    #endregion
}