using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiConnector.Interfaces;
using RobotAppLibraryV2.ApiHandler.Exception;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.Modeles;
using Serilog;
using Range = Moq.Range;

namespace RobotAppLibraryV2.Tests.ApiHandler;

public class ApiHandlerBaseTest
{
    private readonly Mock<ICommandExecutor> _commandExecutor = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly Mock<ApiHandlerBase> apiHandlerBase;


    public ApiHandlerBaseTest()
    {
        _logger.Setup(x => x.ForContext<ApiHandlerBase>())
            .Returns(_logger.Object);


        apiHandlerBase = new Mock<ApiHandlerBase>(MockBehavior.Default, _commandExecutor.Object, _logger.Object)
        {
            CallBase = true
        };
    }

    #region Balance event

    [Fact]
    public void Test_BalanceEvent()
    {
        var caller = false;

        var accountBalance = new AccountBalance
        {
            Balance = 10,
            Credit = 10,
            Equity = 10,
            Margin = 10,
            MarginFree = 10,
            MarginLevel = 10
        };

        apiHandlerBase.Object.NewBalanceEvent += (sender, balance) =>
        {
            caller = true;
            balance.Balance.Should().Be(10);
            balance.Credit.Should().Be(10);
            balance.Equity.Should().Be(10);
            balance.Margin.Should().Be(10);
            balance.MarginFree.Should().Be(10);
            balance.MarginLevel.Should().Be(10);
        };

        _commandExecutor.Raise(x => x.BalanceRecordReceived += null, accountBalance);

        caller.Should().BeTrue();
    }

    #endregion

    #region connect

    [Fact]
    public async void Test_Connect_Async()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(true);

        // Act
        await apiHandlerBase.Object.ConnectAsync(new Credentials());

        // Assert

        _commandExecutor.Verify(x => x.ExecuteLoginCommand(It.IsAny<Credentials>()), Times.Once);
        _commandExecutor.Verify(x => x.ExecuteSubscribeBalanceCommandStreaming(), Times.Once);
        _commandExecutor.Verify(x => x.ExecuteTradesCommandStreaming(), Times.Once);
        _commandExecutor.Verify(x => x.ExecuteTradeStatusCommandStreaming(), Times.Once);
        _commandExecutor.Verify(x => x.ExecuteSubscribeProfitsCommandStreaming(), Times.Once);
        _commandExecutor.Verify(x => x.ExecuteSubscribeNewsCommandStreaming(), Times.Once);
        _commandExecutor.Verify(x => x.ExecutePingCommand(), Times.Between(0, 1, Range.Inclusive));
    }

    #endregion


    #region OnDisconnected

    [Fact]
    public void Test_OnDisconnectedEvent()
    {
        var caller = false;
        apiHandlerBase.Object.Disconnected += (sender, args) => caller = true;

        _commandExecutor.Raise(x => x.Disconnected += null, this, EventArgs.Empty);

        caller.Should().BeTrue();
    }

    #endregion

    #region Disconnect

    [Fact]
    public async void Test_Disconnect_Success()
    {
        // Arrange and Act
        await apiHandlerBase.Object.DisconnectAsync();

        // Assert
        _commandExecutor.Verify(x => x.ExecuteLogoutCommand(), Times.Once);
    }

    [Fact]
    public async void Test_Disconnect_Throw_Exception()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteLogoutCommand()).ThrowsAsync(new Exception());

        // Act
        var act = () => apiHandlerBase.Object.DisconnectAsync();

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
    }

    #endregion

    #region IsConnected

    [Fact]
    public async void Test_IsConnected_Success()
    {
        // Arrange


        //  Act
        var result = apiHandlerBase.Object.IsConnected();

        // assert
        _commandExecutor.Verify(x => x.ExecuteIsConnected(), Times.AtLeastOnce);
    }

    [Fact]
    public async void Test_IsConnected_Throw_Exception()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteIsConnected())
            .Throws(new Exception());

        //  Act
        var result = () => apiHandlerBase.Object.IsConnected();

        // assert
        result.Should().Throw<ApiHandlerException>();
    }

    #endregion

    #region Ping async

    [Fact]
    public async void Test_Ping_IsConnected()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(true);

        // Act
        await apiHandlerBase.Object.PingAsync();

        // Assert
        _commandExecutor.Verify(x => x.ExecutePingCommand(), Times.AtLeastOnce);
        apiHandlerBase.Object.LastPing.Should().NotBeSameDateAs(default);
    }

    [Fact]
    public async void Test_Ping_IsConnected_False()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(false);

        // Act
        await apiHandlerBase.Object.PingAsync();

        // Assert
        _commandExecutor.Verify(x => x.ExecutePingCommand(), Times.Never);
    }

    [Fact]
    public async void Test_Ping_IsConnected_Exception()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(true);

        _commandExecutor.Setup(x => x.ExecutePingCommand()).ThrowsAsync(new Exception());

        // Act
        await apiHandlerBase.Object.PingAsync();

        // Assert
        _logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Exactly(1));
    }

    #endregion

    #region Get balance

    [Fact]
    public async void Test_GetBalance_Success()
    {
        // Arrange and act
        await apiHandlerBase.Object.GetBalanceAsync();

        // Assert
        _commandExecutor.Verify(x => x.ExecuteBalanceAccountCommand(), Times.Once);
    }


    [Fact]
    public async void Test_GetBalance_Throw_Exception()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteBalanceAccountCommand())
            .ThrowsAsync(new Exception());

        // Arrange and act
        var act = () => apiHandlerBase.Object.GetBalanceAsync();

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
    }

    #endregion


    #region GetCalendar

    [Fact]
    public async Task Test_GetCalendarAsync_Success()
    {
        // Arrange
        var expectedCalendarData = new List<CalendarData>
        {
            new()
            {
                /* ... properties ... */
            },
            new()
            {
                /* ... properties ... */
            }
        };
        _commandExecutor.Setup(x => x.ExecuteCalendarCommand())
            .ReturnsAsync(expectedCalendarData);

        // Act
        var calendarData = await apiHandlerBase.Object.GetCalendarAsync();

        // Assert
        calendarData.Should().BeEquivalentTo(expectedCalendarData);
        _commandExecutor.Verify(x => x.ExecuteCalendarCommand(), Times.Once);
    }

    [Fact]
    public async Task Test_GetCalendarAsync_Throw_Exception()
    {
        // Arrange
        _commandExecutor.Setup(x => x.ExecuteCalendarCommand())
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetCalendarAsync();
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteCalendarCommand(), Times.Once);
    }

    #endregion

    #region GetAllSymbols

    [Fact]
    public async Task Test_GetAllSymbolsAsync_Success_WithEmptyCache()
    {
        // Arrange
        var expectedSymbols = new List<SymbolInfo>
        {
            new()
            {
                /* ... properties ... */
            },
            new()
            {
                /* ... properties ... */
            }
        };
        // Assuming AllSymbols is a property or field that can be manipulated for the test
        apiHandlerBase.Object.AllSymbols = new List<SymbolInfo>(); // Start with empty cache
        _commandExecutor.Setup(x => x.ExecuteAllSymbolsCommand())
            .ReturnsAsync(expectedSymbols);

        // Act
        var symbols = await apiHandlerBase.Object.GetAllSymbolsAsync();

        // Assert
        symbols.Should().BeEquivalentTo(expectedSymbols);
        _commandExecutor.Verify(x => x.ExecuteAllSymbolsCommand(), Times.Once);
    }

    [Fact]
    public async Task Test_GetAllSymbolsAsync_Success_WithPopulatedCache()
    {
        // Arrange
        var cachedSymbols = new List<SymbolInfo>
        {
            new()
            {
                /* ... properties ... */
            },
            new()
            {
                /* ... properties ... */
            }
        };
        // Set the cache with already retrieved symbols
        apiHandlerBase.Object.AllSymbols = cachedSymbols;

        // Act
        var symbols = await apiHandlerBase.Object.GetAllSymbolsAsync();

        // Assert
        symbols.Should().BeEquivalentTo(cachedSymbols);
        _commandExecutor.Verify(x => x.ExecuteAllSymbolsCommand(),
            Times.Never); // Command should not be called if cache is populated
    }

    [Fact]
    public async Task Test_GetAllSymbolsAsync_Throw_Exception()
    {
        // Arrange
        apiHandlerBase.Object.AllSymbols = new List<SymbolInfo>(); // Start with empty cache
        _commandExecutor.Setup(x => x.ExecuteAllSymbolsCommand())
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetAllSymbolsAsync();
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteAllSymbolsCommand(), Times.Once);
    }

    #endregion


    #region GetCurrentTradesAsync

    [Fact]
    public async Task Test_GetCurrentTradesAsync_Success()
    {
        // Arrange
        var comment = "testComment";
        var expectedTrades = new Position();
        _commandExecutor.Setup(x => x.ExecuteTradesOpenedTradesCommand(comment))
            .ReturnsAsync(expectedTrades);

        // Act
        var trades = await apiHandlerBase.Object.GetCurrentTradeAsync(comment);

        // Assert
        trades.Should().BeEquivalentTo(expectedTrades);
        _commandExecutor.Verify(x => x.ExecuteTradesOpenedTradesCommand(comment), Times.Once);
    }

    [Fact]
    public async Task Test_GetCurrentTradesAsync_Throw_Exception()
    {
        // Arrange
        var comment = "testComment";
        _commandExecutor.Setup(x => x.ExecuteTradesOpenedTradesCommand(comment))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetCurrentTradeAsync(comment);
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteTradesOpenedTradesCommand(comment), Times.Once);
    }

    #endregion

    #region GetAllPositionsByCommentAsync

    [Fact]
    public async Task Test_GetAllPositionsByCommentAsync_Success()
    {
        // Arrange
        var comment = "testComment";
        var expectedPositions = new List<Position>
        {
            // ...initialize mock positions...
        };
        _commandExecutor.Setup(x => x.ExecuteTradesHistoryCommand(comment))
            .ReturnsAsync(expectedPositions);

        // Act
        var positions = await apiHandlerBase.Object.GetAllPositionsByCommentAsync(comment);

        // Assert
        positions.Should().BeEquivalentTo(expectedPositions);
        _commandExecutor.Verify(x => x.ExecuteTradesHistoryCommand(comment), Times.Once);
    }

    [Fact]
    public async Task Test_GetAllPositionsByCommentAsync_Throw_Exception()
    {
        // Arrange
        var comment = "testComment";
        _commandExecutor.Setup(x => x.ExecuteTradesHistoryCommand(comment))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetAllPositionsByCommentAsync(comment);
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteTradesHistoryCommand(comment), Times.Once);
    }

    #endregion

    #region GetSymbolInformationAsync

    [Fact]
    public async Task Test_GetSymbolInformationAsync_Success_WithCache()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedSymbolInfo = new SymbolInfo { Symbol = symbol };
        apiHandlerBase.Object.AllSymbols = new List<SymbolInfo> { expectedSymbolInfo };

        // Act
        var symbolInfo = await apiHandlerBase.Object.GetSymbolInformationAsync(symbol);

        // Assert
        symbolInfo.Should().BeEquivalentTo(expectedSymbolInfo);
        _commandExecutor.Verify(x => x.ExecuteSymbolCommand(It.IsAny<string>()),
            Times.Never); // Verify that the command executor was not called
    }

    [Fact]
    public async Task Test_GetSymbolInformationAsync_Success_WithoutCache()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedSymbolInfo = new SymbolInfo { Symbol = symbol };
        apiHandlerBase.Object.AllSymbols = new List<SymbolInfo>(); // Cache is empty
        _commandExecutor.Setup(x => x.ExecuteSymbolCommand(symbol))
            .ReturnsAsync(expectedSymbolInfo);

        // Act
        var symbolInfo = await apiHandlerBase.Object.GetSymbolInformationAsync(symbol);

        // Assert
        symbolInfo.Should().BeEquivalentTo(expectedSymbolInfo);
        _commandExecutor.Verify(x => x.ExecuteSymbolCommand(symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetSymbolInformationAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        _commandExecutor.Setup(x => x.ExecuteSymbolCommand(symbol))
            .ThrowsAsync(new Exception());
        apiHandlerBase.Object.AllSymbols = new List<SymbolInfo>(); // Cache is empty

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetSymbolInformationAsync(symbol);
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteSymbolCommand(symbol), Times.Once);
    }

    #endregion

    #region GetTradingHoursAsync

    [Fact]
    public async Task Test_GetTradingHoursAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedTradeHours = new TradeHourRecord
        {
            // ...initialize mock trade hours...
        };
        _commandExecutor.Setup(x => x.ExecuteTradingHoursCommand(symbol))
            .ReturnsAsync(expectedTradeHours);

        // Act
        var tradeHours = await apiHandlerBase.Object.GetTradingHoursAsync(symbol);

        // Assert
        tradeHours.Should().BeEquivalentTo(expectedTradeHours);
        _commandExecutor.Verify(x => x.ExecuteTradingHoursCommand(symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetTradingHoursAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        _commandExecutor.Setup(x => x.ExecuteTradingHoursCommand(symbol))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetTradingHoursAsync(symbol);
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteTradingHoursCommand(symbol), Times.Once);
    }

    #endregion

    #region GetChartAsync

    [Fact]
    public async Task Test_GetChartAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        var expectedCandles = new List<Candle>
        {
            // ...initialize mock candles...
        };
        _commandExecutor.Setup(x => x.ExecuteFullChartCommand(timeframe, It.IsAny<DateTime>(), symbol))
            .ReturnsAsync(expectedCandles);

        // Act
        var candles = await apiHandlerBase.Object.GetChartAsync(symbol, timeframe);

        // Assert
        candles.Should().BeEquivalentTo(expectedCandles);
        _commandExecutor.Verify(x => x.ExecuteFullChartCommand(timeframe, It.IsAny<DateTime>(), symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetChartAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        _commandExecutor.Setup(x => x.ExecuteFullChartCommand(timeframe, It.IsAny<DateTime>(), symbol))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetChartAsync(symbol, timeframe);
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteFullChartCommand(timeframe, It.IsAny<DateTime>(), symbol), Times.Once);
    }

    #endregion

    #region GetChartByDateAsync

    [Fact]
    public async Task Test_GetChartByDateAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 31);
        var expectedCandles = new List<Candle>
        {
            // ...initialize mock candles...
        };
        _commandExecutor.Setup(x => x.ExecuteRangeChartCommand(timeframe, start, end, symbol))
            .ReturnsAsync(expectedCandles);

        // Act
        var candles = await apiHandlerBase.Object.GetChartByDateAsync(symbol, timeframe, start, end);

        // Assert
        candles.Should().BeEquivalentTo(expectedCandles);
        _commandExecutor.Verify(x => x.ExecuteRangeChartCommand(timeframe, start, end, symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetChartByDateAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 31);
        _commandExecutor.Setup(x => x.ExecuteRangeChartCommand(timeframe, start, end, symbol))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await apiHandlerBase.Object.GetChartByDateAsync(symbol, timeframe, start, end);
        await act.Should().ThrowAsync<ApiHandlerException>();

        _commandExecutor.Verify(x => x.ExecuteRangeChartCommand(timeframe, start, end, symbol), Times.Once);
    }

    #endregion

    #region GetTickPriceAsync

    [Fact]
    public async Task Test_GetTickPriceAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedTick = new Tick
        {
            // ...initialize mock tick data...
        };
        _commandExecutor.Setup(x => x.ExecuteTickCommand(symbol))
            .ReturnsAsync(expectedTick);

        // Act
        var tick = await apiHandlerBase.Object.GetTickPriceAsync(symbol);

        // Assert
        Assert.Equal(expectedTick, tick);
        _commandExecutor.Verify(x => x.ExecuteTickCommand(symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetTickPriceAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        _commandExecutor.Setup(x => x.ExecuteTickCommand(symbol))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<ApiHandlerException>(async () =>
            await apiHandlerBase.Object.GetTickPriceAsync(symbol));
        _commandExecutor.Verify(x => x.ExecuteTickCommand(symbol), Times.Once);
    }

    #endregion

    #region OpenPositionAsync

    [Fact]
    public async Task Test_OpenPositionAsync_Success()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _commandExecutor.Setup(x => x.ExecuteOpenTradeCommand(position, price))
            .ReturnsAsync(position);

        // Act
        var result = await apiHandlerBase.Object.OpenPositionAsync(position, price);

        // Assert
        Assert.Equal(position, result);
        _commandExecutor.Verify(x => x.ExecuteOpenTradeCommand(position, price), Times.Once);
        // You may also want to verify that position is added to CachePosition, if that's observable or can be checked.
    }

    [Fact]
    public async Task Test_OpenPositionAsync_Throw_Exception()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _commandExecutor.Setup(x => x.ExecuteOpenTradeCommand(position, price))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<ApiHandlerException>(async () =>
            await apiHandlerBase.Object.OpenPositionAsync(position, price));
        _commandExecutor.Verify(x => x.ExecuteOpenTradeCommand(position, price), Times.Once);
        // Additional check can be done to ensure CachePosition doesn't contain the position after the exception.
    }

    #endregion

    #region UpdatePositionAsync

    [Fact]
    public async Task Test_UpdatePositionAsync_Success()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _commandExecutor.Setup(x => x.ExecuteUpdateTradeCommand(position, price))
            .ReturnsAsync(new Position());

        // Act
        await apiHandlerBase.Object.UpdatePositionAsync(price, position);

        // Assert
        _commandExecutor.Verify(x => x.ExecuteUpdateTradeCommand(position, price), Times.Once);
    }

    [Fact]
    public async Task Test_UpdatePositionAsync_Throw_Exception()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _commandExecutor.Setup(x => x.ExecuteUpdateTradeCommand(position, price))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<ApiHandlerException>(async () =>
            await apiHandlerBase.Object.UpdatePositionAsync(price, position));
        _commandExecutor.Verify(x => x.ExecuteUpdateTradeCommand(position, price), Times.Once);
    }

    #endregion

    #region ClosePositionAsync

    [Fact]
    public async Task Test_ClosePositionAsync_Success()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _commandExecutor.Setup(x => x.ExecuteCloseTradeCommand(position, price))
            .ReturnsAsync(new Position());

        // Act
        await apiHandlerBase.Object.ClosePositionAsync(price, position);

        // Assert
        _commandExecutor.Verify(x => x.ExecuteCloseTradeCommand(position, price), Times.Once);
    }

    [Fact]
    public async Task Test_ClosePositionAsync_Throw_Exception()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _commandExecutor.Setup(x => x.ExecuteCloseTradeCommand(position, price))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var act = () => apiHandlerBase.Object.ClosePositionAsync(price, position);
        await act.Should().ThrowAsync<ApiHandlerException>();
    }

    #endregion

    #region SubscribePrice

    [Fact]
    public void Test_SubscribePrice_Success()
    {
        // Arrange
        var symbol = "AAPL";
        _commandExecutor.Setup(x => x.ExecuteTickPricesCommandStreaming(symbol));

        // Act
        apiHandlerBase.Object.SubscribePrice(symbol);

        // Assert
        _commandExecutor.Verify(x => x.ExecuteTickPricesCommandStreaming(symbol), Times.Once);
    }

    [Fact]
    public void Test_SubscribePrice_Throw_Exception()
    {
        // Arrange
        var symbol = "AAPL";
        _commandExecutor.Setup(x => x.ExecuteTickPricesCommandStreaming(symbol))
            .Throws(new Exception());

        // Act & Assert
        var ex = Assert.Throws<ApiHandlerException>(() => apiHandlerBase.Object.SubscribePrice(symbol));
        Assert.Contains($"Error on  {nameof(apiHandlerBase.Object.SubscribePrice)}", ex.Message);
        _commandExecutor.Verify(x => x.ExecuteTickPricesCommandStreaming(symbol), Times.Once);
    }

    #endregion

    #region UnsubscribePrice

    [Fact]
    public void Test_UnsubscribePrice_Success()
    {
        // Arrange
        var symbol = "AAPL";
        _commandExecutor.Setup(x => x.ExecuteStopTickPriceCommandStreaming(symbol));

        // Act
        apiHandlerBase.Object.UnsubscribePrice(symbol);

        // Assert
        _commandExecutor.Verify(x => x.ExecuteStopTickPriceCommandStreaming(symbol), Times.Once);
    }

    [Fact]
    public void Test_UnsubscribePrice_Throw_Exception()
    {
        // Arrange
        var symbol = "AAPL";
        _commandExecutor.Setup(x => x.ExecuteStopTickPriceCommandStreaming(symbol))
            .Throws(new Exception());

        // Act & Assert
        var ex = Assert.Throws<ApiHandlerException>(() => apiHandlerBase.Object.UnsubscribePrice(symbol));
        Assert.Contains($"Error on  {nameof(apiHandlerBase.Object.UnsubscribePrice)}", ex.Message);
        _commandExecutor.Verify(x => x.ExecuteStopTickPriceCommandStreaming(symbol), Times.Once);
    }

    #endregion

    #region Position state event

    // [Fact]
    // public void Test_NoCall_PositionState_No_Custom_Comment()
    // {
    //     // Arrange
    //     var caller = false;
    //     var position = new Position
    //     {
    //         StatusPosition = StatusPosition.Open
    //     };
    //
    //     apiHandlerBase.Object.PositionRejectedEvent += (sender, position1) => caller = true;
    //     apiHandlerBase.Object.PositionOpenedEvent += (sender, position1) => caller = true;
    //     apiHandlerBase.Object.PositionUpdatedEvent += (sender, position1) => caller = true;
    //     apiHandlerBase.Object.PositionClosedEvent += (sender, position1) => caller = true;
    //
    //     // Act
    //     _commandExecutor.Raise(x => x.TcpStreamingConnector.TradeRecordReceived += null, position);
    //
    //     // Assert
    //     caller.Should().BeFalse();
    // }

    [Fact]
    public void Test_PositionState_Pending()
    {
        // Arrange
        var caller = false;
        var position = new Position
        {
            StatusPosition = StatusPosition.Pending
        };

        apiHandlerBase.Object.PositionRejectedEvent += (sender, position1) => caller = true;
        apiHandlerBase.Object.PositionOpenedEvent += (sender, position1) => caller = true;
        apiHandlerBase.Object.PositionUpdatedEvent += (sender, position1) => caller = true;
        apiHandlerBase.Object.PositionClosedEvent += (sender, position1) => caller = true;

        // Act
        _commandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeFalse();
    }

    [Fact]
    public async Task Test_PositionState_Open()
    {
        // Arrange
        var caller = false;
        var position = new Position
        {
            StatusPosition = StatusPosition.Open,
            StrategyId = "1",
            Id = "1"
        };

        _commandExecutor.Setup(x => x.ExecuteOpenTradeCommand(It.IsAny<Position>(), It.IsAny<decimal>()))
            .ReturnsAsync(new Position
            {
                StrategyId = "1",
                Id = "1"
            });

        await apiHandlerBase.Object.OpenPositionAsync(new Position(), 1);


        apiHandlerBase.Object.PositionOpenedEvent += (sender, position1) => caller = true;


        // Act
        _commandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }

    [Fact]
    public void Test_PositionState_Update()
    {
        // Arrange
        var caller = false;
        var position = new Position
        {
            StatusPosition = StatusPosition.Updated,
            StrategyId = "1",
            Id = "1"
        };


        apiHandlerBase.Object.PositionUpdatedEvent += (sender, position1) => caller = true;

        var position1 = new Position
        {
            StatusPosition = StatusPosition.Open,
            StrategyId = "1",
            Id = "1"
        };
        _commandExecutor.Raise(x => x.TradeRecordReceived += null, position1);

        // Act
        _commandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }

    [Fact]
    public async Task Test_PositionState_Close()
    {
        // Arrange
        var caller = false;
        var position = new Position
        {
            StatusPosition = StatusPosition.Close,
            StrategyId = "1",
            Id = "1"
        };

        _commandExecutor.Setup(x => x.ExecuteOpenTradeCommand(It.IsAny<Position>(), It.IsAny<decimal>()))
            .ReturnsAsync(new Position
            {
                StrategyId = "1",
                Id = "1"
            });

        await apiHandlerBase.Object.OpenPositionAsync(new Position(), 1);


        apiHandlerBase.Object.PositionClosedEvent += (sender, position1) => caller = true;

        var position1 = new Position
        {
            StatusPosition = StatusPosition.Open,
            StrategyId = "1",
            Id = "1"
        };
        _commandExecutor.Raise(x => x.TradeRecordReceived += null, position1);


        // Act
        _commandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }

    #endregion
}