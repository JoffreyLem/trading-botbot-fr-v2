using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Exception;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.ApiHandler.Xtb;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;
using RobotAppLibraryV2.ApiHandler.Xtb.records;
using RobotAppLibraryV2.ApiHandler.Xtb.responses;
using RobotAppLibraryV2.ApiHandler.Xtb.sync;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;
using Serilog;

namespace RobotAppLibraryV2.Tests.ApiHandler;

public class XtbApiTest
{
    private const string RessourcePath = "RobotAppLibraryV2.Tests.ApiHandler.Ressources.Xtb";

    private readonly Mock<IApiCommandFactory> _apiCommandFactoryXtb = new();
    private readonly Mock<ISyncApiConnector> _connector = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly Mock<IStreamingApiConnector> _streamingApiConnector = new();
    private readonly XtbApi _xtbApi;

    public XtbApiTest()
    {
        _logger.Setup(x => x.ForContext<XtbApi>())
            .Returns(_logger.Object);

        _connector.SetupGet(x => x.StreamingApiConnector).Returns(_streamingApiConnector.Object);
        _xtbApi = new XtbApi(_apiCommandFactoryXtb.Object, _connector.Object, _logger.Object);
    }

    #region Unsubscribe price

    [Fact]
    public void Test_UnSubscribe_price_success()
    {
        // Arrange and act

        _xtbApi.UnsubscribePrice("test");

        _streamingApiConnector.Verify(x => x.UnsubscribePrice(It.IsAny<string>()), Times.Exactly(1));
    }

    #endregion

    #region Balance record received

    [Fact]
    public async void Test_BalanceRecordReceived_success()
    {
        // Arrange
        ConnectMock();
        var caller = false;
        var streamingBalanceRecord = new StreamingBalanceRecord();
        streamingBalanceRecord.Balance = 10;
        streamingBalanceRecord.Credit = 10;
        streamingBalanceRecord.Equity = 10;
        streamingBalanceRecord.Margin = 10;
        streamingBalanceRecord.MarginFree = 10;
        streamingBalanceRecord.MarginLevel = 10;

        _xtbApi.NewBalanceEvent += (_, balance) =>
        {
            // Event assert
            caller = true;
            balance.Balance.Should().Be(10);
            balance.Credit.Should().Be(10);
            balance.Equity.Should().Be(10);
            balance.Margin.Should().Be(10);
            balance.MarginFree.Should().Be(10);
            balance.MarginLevel.Should().Be(10);
        };

        // Act
        _streamingApiConnector.Raise(x => x.BalanceRecordReceived += null, streamingBalanceRecord);

        // Assert
        caller.Should().BeTrue();
        _xtbApi.AccountBalance.Balance.Should().Be(10);
        _xtbApi.AccountBalance.Credit.Should().Be(10);
        _xtbApi.AccountBalance.Equity.Should().Be(10);
        _xtbApi.AccountBalance.Margin.Should().Be(10);
        _xtbApi.AccountBalance.MarginFree.Should().Be(10);
        _xtbApi.AccountBalance.MarginLevel.Should().Be(10);
    }

    #endregion

    #region News record received test

    [Fact]
    public async void Test_NewsRecordReceived()
    {
        // Arrange
        ConnectMock();
        var caller = false;
        var streamingNewsRecord = new StreamingNewsRecord
        {
            Body = "test",
            Key = "test",
            Title = "test",
            Time = 0
        };

        _xtbApi.NewsEvent += (_, news) =>
        {
            // Event assert
            caller = true;
            news.Body.Should().Be("test");
            news.Key.Should().Be("test");
            news.Title.Should().Be("test");
            news.Time.Should().Be(0);
        };

        // Act
        _streamingApiConnector.Raise(x => x.NewsRecordReceived += null, streamingNewsRecord);

        // Assert
        caller.Should().BeTrue();
    }

    #endregion

    #region ProfitRecordUpdate

    [Fact]
    public async Task test_UpdatePositionProfit()
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();

        var caller = false;
        var streamingProfitRecord = new StreamingProfitRecord
        {
            Order = 10,
            Order2 = 20,
            Position = 30,
            Profit = 5000
        };

        _xtbApi.PositionUpdatedEvent += (_, position) =>
        {
            // Assert test
            caller = true;
            position.Profit.Should().Be(5000);
        };

        // Act
        _streamingApiConnector.Raise(x => x.ProfitRecordReceived += null, streamingProfitRecord);

        // Assert
        caller.Should().BeTrue();
    }

    #endregion


    // For settings the callback connector streaming.
    private void ConnectMock()
    {
        _xtbApi.ConnectAsync("test", "test").GetAwaiter().GetResult();
    }

    #region Subscribe price

    [Fact]
    public void Test_Subscribe_price_success()
    {
        // Arrange and act

        _xtbApi.SubscribePrice("test");

        _streamingApiConnector.Verify(x => x.SubscribePrice(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<long?>()),
            Times.Exactly(1));
    }

    #endregion

    #region Connect

    [Fact]
    public async Task Test_Connection_Success()
    {
        // Act and arrange
        await _xtbApi.ConnectAsync("test", "test");

        // Assert
        _connector.Verify(x => x.Connect(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.Connect(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.SubscribeKeepAlive(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.SubscribeBalance(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.SubscribeTrades(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.SubscribeTradeStatus(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.SubscribeProfits(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.SubscribeNews(), Times.Exactly(1));
        _apiCommandFactoryXtb.Verify
        (x => x.ExecuteLoginCommand(_connector.Object, It.Is<Credentials>(credentials => credentials.Login == "test" && credentials.Password == "test"), It.IsAny<bool>()),
            Times.Exactly(1));
    }


    [Fact]
    public async Task Test_Connection_Failed()
    {
        // Arrange

        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteLoginCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<Credentials>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        var act = async () => await _xtbApi.ConnectAsync("test", "test");

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        _connector.Verify(x => x.Connect(), Times.Once);
        _streamingApiConnector.Verify(x => x.Connect(), Times.Never);
        _streamingApiConnector.Verify(x => x.SubscribeKeepAlive(), Times.Never);
        _streamingApiConnector.Verify(x => x.SubscribeBalance(), Times.Never);
        _streamingApiConnector.Verify(x => x.SubscribeTrades(), Times.Never);
        _streamingApiConnector.Verify(x => x.SubscribeTradeStatus(), Times.Never);
        _streamingApiConnector.Verify(x => x.SubscribeProfits(), Times.Never);
        _streamingApiConnector.Verify(x => x.SubscribeNews(), Times.Never);
    }

    [Fact]
    public async Task Test_Connection_subscribtion_Failed()
    {
        // Arrange 
        _streamingApiConnector.Setup(x => x.SubscribeKeepAlive()).Throws(new Exception());

        // Act
        var act = async () => await _xtbApi.ConnectAsync("test", "test");

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.AtLeast(2));
        _connector.Verify(x => x.Connect(), Times.Once);
        _streamingApiConnector.Verify(x => x.Connect(), Times.Once);
    }

    #endregion

    #region Disconnect

    [Fact]
    public async Task Test_Disconnection_Success_NoPositions()
    {
        // Arrange and Act

        var tradesResponse = TestUtils.FileReadContent(RessourcePath, "currentTrade.json");
        var tradeTransactionResponse = new TradesResponse(tradesResponse);
        tradeTransactionResponse.TradeRecords.Clear();
        _apiCommandFactoryXtb.Setup(x => x.ExecuteTradesCommand(It.IsAny<ISyncApiConnector>(), true, It.IsAny<bool>()))
            .Returns(tradeTransactionResponse);
        await _xtbApi.DisconnectAsync();

        // Assert
        _connector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeBalance(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeTrades(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeTradeStatus(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeProfits(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeNews(), Times.Exactly(1));
    }

    [Fact]
    public async Task Test_Disconnection_Success_Positions()
    {
        // Arrange 
        ConnectMock();
        var tradesResponse = TestUtils.FileReadContent(RessourcePath, "currentTrade.json");
        var tradeTransactionResponse = new TradesResponse(tradesResponse);

        _apiCommandFactoryXtb.Setup(x => x.ExecuteTradesCommand(It.IsAny<ISyncApiConnector>(), true, It.IsAny<bool>()))
            .Returns(tradeTransactionResponse);

        var tickPriceData = TestUtils.FileReadContent(RessourcePath, "tickPrice.json");
        var tickPricesResponse = new TickPricesResponse(tickPriceData);
        _apiCommandFactoryXtb.Setup(x => x.ExecuteTickPricesCommand(It.IsAny<ISyncApiConnector>(),
                It.IsAny<List<string>>(), It.IsAny<long?>(), It.IsAny<bool>()))
            .Returns(tickPricesResponse);

        await test_positionOpenedCallback_good_orderRef();

        // Act
        await _xtbApi.DisconnectAsync();

        // Assert
        _connector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeBalance(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeTrades(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeTradeStatus(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeProfits(), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.UnsubscribeNews(), Times.Exactly(1));
        _apiCommandFactoryXtb.Verify(x =>
            x.ExecuteTradeTransactionCommand(
                It.IsAny<ISyncApiConnector>(),
                It.Is<TradeTransInfoRecord>(record => record.Type == TRADE_TRANSACTION_TYPE.ORDER_CLOSE),
                It.IsAny<bool>()), Times.Exactly(1));
    }

    [Fact]
    public async Task Test_Disconnection_Error_ConnectorDisconnect()
    {
        // Arrange

        var tradesResponse = TestUtils.FileReadContent(RessourcePath, "currentTrade.json");
        var tradeTransactionResponse = new TradesResponse(tradesResponse);
        tradeTransactionResponse.TradeRecords.Clear();
        _apiCommandFactoryXtb.Setup(x => x.ExecuteTradesCommand(It.IsAny<ISyncApiConnector>(), true, It.IsAny<bool>()))
            .Returns(tradeTransactionResponse);
        _connector.Setup(x => x.Disconnect(It.IsAny<bool>()))
            .Throws(new Exception());

        // Act and assert
        var act = async () => await _xtbApi.DisconnectAsync();
        await act.Should().ThrowAsync<ApiHandlerException>();
        _connector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(1));
        _streamingApiConnector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(1));
    }

    [Fact]
    public async Task Test_Disconnection_Error_StreamingConnector_disconnect()
    {
        // Arrange

        var tradesResponse = TestUtils.FileReadContent(RessourcePath, "currentTrade.json");
        var tradeTransactionResponse = new TradesResponse(tradesResponse);
        tradeTransactionResponse.TradeRecords.Clear();
        _apiCommandFactoryXtb.Setup(x => x.ExecuteTradesCommand(It.IsAny<ISyncApiConnector>(), true, It.IsAny<bool>()))
            .Returns(tradeTransactionResponse);
        _streamingApiConnector.Setup(x => x.Disconnect(It.IsAny<bool>()))
            .Throws(new Exception());

        // Act and assert
        var act = async () => await _xtbApi.DisconnectAsync();
        await act.Should().ThrowAsync<ApiHandlerException>();
        _connector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(0));
        _streamingApiConnector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(1));
    }

    [Fact]
    public async Task Test_Disconnection_Error_Disable_Streaming()
    {
        // Arrange
        _streamingApiConnector.Setup(x => x.UnsubscribeBalance())
            .Throws(new Exception());

        // Act and assert
        var act = async () => await _xtbApi.DisconnectAsync();
        await act.Should().ThrowAsync<ApiHandlerException>();
        _connector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(0));
        _streamingApiConnector.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Exactly(0));
    }

    #endregion

    #region IsConnected

    // TODO : A Remplir

    #endregion

    #region Ping

    [Fact]
    public async Task Test_Ping()
    {
        // Arrange 
        _apiCommandFactoryXtb.Setup(x => x.ExecutePingCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()));

        // Act 
        await _xtbApi.PingAsync();

        // Assert 
        _logger.Verify(x => x.Information(It.IsAny<string>()), Times.Exactly(1));
    }

    [Fact]
    public async Task Test_Ping_Error()
    {
        // Arrange 
        _apiCommandFactoryXtb.Setup(x => x.ExecutePingCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act 
        await _xtbApi.PingAsync();

        // Assert 
        _logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Exactly(1));
    }

    #endregion

    #region GetBalance

    [Fact]
    public async Task Test_GetBalance_Success()
    {
        // Arrange 
        var marginLevel = TestUtils.FileReadContent(RessourcePath, "marginlevel.json");
        var allSymbolsResponse = new MarginLevelResponse(marginLevel);

        _apiCommandFactoryXtb.Setup(x => x.ExecuteMarginLevelCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Returns(allSymbolsResponse);

        // Act
        var balance = await _xtbApi.GetBalanceAsync();

        // Assert
        balance.Balance.Should().Be(10000);
        balance.Credit.Should().Be(10000);
        balance.Equity.Should().Be(10000);
        balance.Margin.Should().Be(10000);
        balance.MarginFree.Should().Be(10000);
        balance.MarginLevel.Should().Be(10000);
    }

    [Fact]
    public async Task Test_GetBalance_Success_AccountBalance_Account_Updated()
    {
        // Arrange 
        var marginLevel = TestUtils.FileReadContent(RessourcePath, "marginlevel.json");
        var allSymbolsResponse = new MarginLevelResponse(marginLevel);

        _apiCommandFactoryXtb.Setup(x => x.ExecuteMarginLevelCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Returns(allSymbolsResponse);

        // Act
        await _xtbApi.GetBalanceAsync();

        // Assert
        _xtbApi.AccountBalance.Balance.Should().Be(10000);
        _xtbApi.AccountBalance.Credit.Should().Be(10000);
        _xtbApi.AccountBalance.Equity.Should().Be(10000);
        _xtbApi.AccountBalance.Margin.Should().Be(10000);
        _xtbApi.AccountBalance.MarginFree.Should().Be(10000);
        _xtbApi.AccountBalance.MarginLevel.Should().Be(10000);
    }

    [Fact]
    public async Task Test_GetBalance_Exception()
    {
        // Arrange
        _apiCommandFactoryXtb.Setup(x => x.ExecuteMarginLevelCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        Func<Task<AccountBalance>> act = async () => await _xtbApi.GetBalanceAsync();

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>()));
    }

    #endregion

    #region GetAllPositions

    [Fact]
    public async Task GetAllPositions_ReturnsPositions()
    {
        // Arrange
        var tradeHistorystr = TestUtils.FileReadContent(RessourcePath, "getTradesHistory.json");
        var tradeHistory = new TradesHistoryResponse(tradeHistorystr);

        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteTradesHistoryCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<long?>(), It.IsAny<long?>(),
                    It.IsAny<bool>()))
            .Returns(tradeHistory);

        // Act
        var result = await _xtbApi.GetAllPositionsAsync();

        // Assert

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        var position = result[0];
        position.TypePosition.Should().Be(TypePosition.Buy);
        position.Profit.Should().Be(100);
        position.OpenPrice.Should().Be(100);
        position.ClosePrice.Should().Be(100);
        position.DateOpen.Should()
            .Be(tradeHistory.TradeRecords.First!.Value.Open_time.GetValueOrDefault().ConvertToDatetime());
        position.DateClose.Should()
            .Be(tradeHistory.TradeRecords.First.Value.Close_time.GetValueOrDefault().ConvertToDatetime());
        position.ReasonClosed.Should().Be(ReasonClosed.Sl.ToString());
        position.StopLoss.Should().Be(100);
        position.TakeProfit.Should().Be(100);
        position.Volume.Should().Be(100);
        position.StatusPosition.Should().Be(StatusPosition.Close);
        position.Comment.Should().Be("[S/L]");
        position.PositionId.Should().Be(100);
        position.Order.Should().Be(100);
        position.Order2.Should().Be(100);
        position.Symbol.Should().Be("DE30");
        position.CustomComment.Should().Be(tradeHistory.TradeRecords.First().CustomComment);
    }

    [Fact]
    public async Task GetAllPositions_ThrowsApiCommunicationErrorException_OnError()
    {
        // Arrange

        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteTradesHistoryCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<long?>(), It.IsAny<long?>(),
                    It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        Func<Task<List<Position>>> result = async () => await _xtbApi.GetAllPositionsAsync();

        // Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region GetCalendar

    [Fact]
    public async Task test_getCalendar_success()
    {
        // Arrange
        var calendarResponseStr = TestUtils.FileReadContent(RessourcePath, "getTradesHistory.json");
        var calendarResponse = new CalendarResponse(calendarResponseStr);

        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteCalendarCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Returns(calendarResponse);

        // Act
        var result = await _xtbApi.GetCalendarAsync();

        // Assert

        var firstData = result[0];
        var firstDataFromApi = calendarResponse.CalendarRecords.First();

        firstData.Should().NotBeNull();
        firstData.Country.Should().Be(firstDataFromApi.Country);
        firstData.Current.Should().Be(firstDataFromApi.Current);
        firstData.Forecast.Should().Be(firstDataFromApi.Forecast);
        firstData.Impact.Should().Be(firstDataFromApi.Impact);
        firstData.Period.Should().Be(firstDataFromApi.Period);
        firstData.Previous.Should().Be(firstDataFromApi.Previous);
        firstData.Time.Should().Be(firstDataFromApi.Time.GetValueOrDefault().ConvertToDatetime());
        firstData.Title.Should().Be(firstDataFromApi.Title);
    }

    [Fact]
    public async Task test_getCalendar_Error()
    {
        // Arrange
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteCalendarCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        Func<Task<List<Calendar>>> result = async () => await _xtbApi.GetCalendarAsync();

        // Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Get All Symbols

    [Fact]
    public async Task Test_GetAllSymbol_Success()
    {
        // Arrange
        var allsymbol = TestUtils.FileReadContent(RessourcePath, "allsymbolrsp.json");
        var allSymbolsResponse = new AllSymbolsResponse(allsymbol);

        _apiCommandFactoryXtb.Setup(x => x.ExecuteAllSymbolsCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Returns(allSymbolsResponse);

        // Act 
        var result = await _xtbApi.GetAllSymbolsAsync();

        // Assert 

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[1].Should().Be("EURUSD");
    }

    [Fact]
    public async Task Test_GetAllSymbol_Error()
    {
        // Arrange
        _apiCommandFactoryXtb.Setup(x => x.ExecuteAllSymbolsCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act 
        Func<Task<List<string>>> act = async () => await _xtbApi.GetAllSymbolsAsync();

        // Assert 
        await act.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Get current trades

    [Fact]
    public async Task Test_GetCurrentTrades_Success()
    {
        // Arrange
        var tradesResponse = TestUtils.FileReadContent(RessourcePath, "currentTrade.json");
        var tradeTransactionResponse = new TradesResponse(tradesResponse);

        _apiCommandFactoryXtb.Setup(x => x.ExecuteTradesCommand(It.IsAny<ISyncApiConnector>(), true, It.IsAny<bool>()))
            .Returns(tradeTransactionResponse);

        // Act

        var result = await _xtbApi.GetCurrentTradesAsync();

        // Assert

        result.Count.Should().Be(2);
        var position = result[0];
        position.TypePosition.Should().Be(TypePosition.Buy);
        position.Profit.Should().Be(-0.63M);
        position.OpenPrice.Should().Be(16501.05M);
        position.ClosePrice.Should().Be(16434.95M);
        position.DateOpen.Should().Be(tradeTransactionResponse.TradeRecords.First!.Value.Open_time.GetValueOrDefault()
            .ConvertToDatetime());
        position.DateClose.Should().Be(tradeTransactionResponse.TradeRecords.First.Value.Close_time.GetValueOrDefault()
            .ConvertToDatetime());
        position.ReasonClosed.Should().Be(null);
        position.StopLoss.Should().Be(10);
        position.TakeProfit.Should().Be(12);
        position.Volume.Should().Be(0.01);
        position.StatusPosition.Should().Be(StatusPosition.Open);
        position.Comment.Should().Be("");
        position.PositionId.Should().Be(10);
        position.Order.Should().Be(10);
        position.Order2.Should().Be(20);
        position.Symbol.Should().Be("BITCOIN");
        position.CustomComment.Should().Be("test");
    }

    [Fact]
    public async Task Test_GetCurrentTrades_Error()
    {
        // Arrange

        _apiCommandFactoryXtb.Setup(x => x.ExecuteTradesCommand(It.IsAny<ISyncApiConnector>(), true, It.IsAny<bool>()))
            .Throws(new Exception());

        // Act

        Func<Task<List<Position>>> act = async () => await _xtbApi.GetCurrentTradesAsync();

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Get all positions by comment

    [Fact]
    public async Task Test_GetAllPositionByComment_Success()
    {
        // Arrange
        var tradeHistorystr = TestUtils.FileReadContent(RessourcePath, "getTradesHistory.json");
        var tradeHistory = new TradesHistoryResponse(tradeHistorystr);

        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteTradesHistoryCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<long?>(), It.IsAny<long?>(),
                    It.IsAny<bool>()))
            .Returns(tradeHistory);

        // Act
        var result = await _xtbApi.GetAllPositionsByCommentAsync("test");

        //Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        var position = result[0];
        position.TypePosition.Should().Be(TypePosition.Buy);
        position.Profit.Should().Be(100);
        position.OpenPrice.Should().Be(100);
        position.ClosePrice.Should().Be(100);
        position.DateOpen.Should()
            .Be(tradeHistory.TradeRecords.First!.Value.Open_time.GetValueOrDefault().ConvertToDatetime());
        position.DateClose.Should()
            .Be(tradeHistory.TradeRecords.First.Value.Close_time.GetValueOrDefault().ConvertToDatetime());
        position.ReasonClosed.Should().Be(ReasonClosed.Sl.ToString());
        position.StopLoss.Should().Be(100);
        position.TakeProfit.Should().Be(100);
        position.Volume.Should().Be(100);
        position.StatusPosition.Should().Be(StatusPosition.Close);
        position.Comment.Should().Be("[S/L]");
        position.PositionId.Should().Be(100);
        position.Order.Should().Be(100);
        position.Order2.Should().Be(100);
        position.Symbol.Should().Be("DE30");
        position.CustomComment.Should().Be(tradeHistory.TradeRecords.First().CustomComment);
    }

    [Fact]
    public async Task Test_GetAllPositionByComment_Error()
    {
        // Arrange

        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteTradesHistoryCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<long?>(), It.IsAny<long?>(),
                    It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        Func<Task<List<Position>>> result = async () => await _xtbApi.GetAllPositionsByCommentAsync("test");

        //Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Exactly(2));
    }

    #endregion

    #region Get symbol information

    [Fact]
    public async Task Test_get_symbolinfo_success()
    {
        // Arrange
        var symbolresponsedata = TestUtils.FileReadContent(RessourcePath, "symbolInfo.json");
        var symbolresponse = new SymbolResponse(symbolresponsedata);
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(symbolresponse);

        // act 
        var symbolToReturn = await _xtbApi.GetSymbolInformationAsync("EURUSD");

        // Assert
        var firstSymbolApi = symbolresponse.Symbol;
        symbolToReturn.Should().NotBeNull();
        symbolToReturn.Category.Should().Be(Category.Forex);
        symbolToReturn.ContractSize.Should().Be(firstSymbolApi.ContractSize);
        symbolToReturn.Currency1.Should().Be(firstSymbolApi.Currency);
        symbolToReturn.Currency2.Should().Be(firstSymbolApi.CurrencyProfit);
        symbolToReturn.LotMax.Should().Be(firstSymbolApi.LotMax);
        symbolToReturn.LotMin.Should().Be(firstSymbolApi.LotMin);
        symbolToReturn.Precision.Should().Be(firstSymbolApi.Precision);
        symbolToReturn.Symbol.Should().Be(firstSymbolApi.Symbol);
        symbolToReturn.TickValue.Should().Be(firstSymbolApi.TickValue);
        symbolToReturn.TickSize.Should().Be(firstSymbolApi.TickSize);
    }

    [Fact]
    public async Task Test_get_symbolinfo_error()
    {
        // Arrange

        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // act 
        Func<Task<SymbolInfo>> act = async () => await _xtbApi.GetSymbolInformationAsync("EURUSD");

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<string>()));
    }

    #endregion

    #region Get trading hours

    [Fact]
    public async Task Test_GetTradingHours_Success()
    {
        // Arrange
        var hoursResponseData = TestUtils.FileReadContent(RessourcePath, "tradingHours.json");
        var hourResponse = new TradingHoursResponse(hoursResponseData);
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteTradingHoursCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
            .Returns(hourResponse);

        // Act
        var result = await _xtbApi.GetTradingHoursAsync("DE30");

        var dateToCheck = 1;
        // Assert
        foreach (var resultHoursRecord in result.HoursRecords.OrderBy(x => x.Day))
        {
            resultHoursRecord.Day.Should().Be((DayOfWeek)dateToCheck);
            resultHoursRecord.From.Should().Be(TimeSpan.FromMilliseconds(8100000));
            resultHoursRecord.To.Should().Be(TimeSpan.FromMilliseconds(79200000));
            dateToCheck++;
        }
    }

    [Fact]
    public async Task Test_GetTradingHours_Fail()
    {
        // Arrange
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteTradingHoursCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        Func<Task<TradeHourRecord>> result = async () => await _xtbApi.GetTradingHoursAsync("DE30");

        // Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<string>()));
    }

    #endregion

    #region GetChart

    [Fact]
    public async Task Test_GetChart_Success()
    {
        // Arrange 
        var symbolresponsedata = TestUtils.FileReadContent(RessourcePath, "symbolInfo.json");
        var symbolresponse = new SymbolResponse(symbolresponsedata);
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(symbolresponse);

        var chartData = TestUtils.FileReadContent(RessourcePath, "chart.json");
        var chartLastResponse = new ChartLastResponse(chartData);
        _apiCommandFactoryXtb.Setup(x => x.ExecuteChartLastCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(),
                It.IsAny<PERIOD_CODE>(), It.IsAny<long?>(), It.IsAny<bool>()))
            .Returns(chartLastResponse);

        // act

        var result = await _xtbApi.GetChartAsync("EURUSD", Timeframe.OneMinute);

        // Assert

        var tickSize = 0.00001M;

        var firstResult = result.First();
        var firstCharLast = chartLastResponse.RateInfos.First();
        var open = (decimal)(firstCharLast.Open.GetValueOrDefault() * (double)tickSize);
        firstResult.Open.Should().Be((decimal)(firstCharLast.Open.GetValueOrDefault() * (double)tickSize));
        firstResult.High.Should().Be(open + (decimal)(firstCharLast.High.GetValueOrDefault() * (double)tickSize));
        firstResult.Low.Should().Be(open + (decimal)(firstCharLast.Low.GetValueOrDefault() * (double)tickSize));
        firstResult.Close.Should().Be(open + (decimal)(firstCharLast.Close.GetValueOrDefault() * (double)tickSize));
        firstResult.Volume.Should().Be((decimal)firstCharLast.Vol.GetValueOrDefault());
        var dateToTchec = new DateTime(2022, 11, 24, 20, 25, 00);
        firstResult.Date.Should().Be(dateToTchec);
    }

    [Fact]
    public async Task Test_GetChart_Error()
    {
        // Arrange 
        var symbolresponsedata = TestUtils.FileReadContent(RessourcePath, "symbolInfo.json");
        var symbolresponse = new SymbolResponse(symbolresponsedata);
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(symbolresponse);

        var chartData = TestUtils.FileReadContent(RessourcePath, "chart.json");
        new ChartLastResponse(chartData);
        _apiCommandFactoryXtb.Setup(x => x.ExecuteChartLastCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(),
                It.IsAny<PERIOD_CODE>(), It.IsAny<long?>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // act

        Func<Task<List<Candle>>> result = async () => await _xtbApi.GetChartAsync("EURUSD", Timeframe.OneMinute);

        // Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Test_GetChart_Error_Symbols()
    {
        // Arrange 
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // act
        Func<Task<List<Candle>>> result = async () => await _xtbApi.GetChartAsync("EURUSD", Timeframe.OneMinute);

        // Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Exactly(1));
        _logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
    }

    #endregion

    #region Get chart by date

    [Fact]
    public async Task Test_GetChart_bydate_Success()
    {
        // Arrange 
        var symbolresponsedata = TestUtils.FileReadContent(RessourcePath, "symbolInfo.json");
        var symbolresponse = new SymbolResponse(symbolresponsedata);
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(symbolresponse);

        var chartData = TestUtils.FileReadContent(RessourcePath, "chart.json");
        var chartRangeResponse = new ChartRangeResponse(chartData);
        _apiCommandFactoryXtb.Setup(x => x.ExecuteChartRangeCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(),
                It.IsAny<PERIOD_CODE>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<bool>()))
            .Returns(chartRangeResponse);

        // act
        var dateStart = new DateTime(2022, 11, 24, 20, 25, 00);
        var dateEnd = new DateTime(2022, 11, 25, 20, 55, 00);

        var result = await _xtbApi.GetChartByDateAsync("EURUSD", Timeframe.OneMinute, dateStart, dateEnd);

        // Assert

        var tickSize = 0.00001M;

        var firstResult = result.First();
        var firstCharLast = chartRangeResponse.RateInfos.First();
        var open = (decimal)(firstCharLast.Open.GetValueOrDefault() * (double)tickSize);
        firstResult.Open.Should().Be((decimal)(firstCharLast.Open.GetValueOrDefault() * (double)tickSize));
        firstResult.High.Should().Be(open + (decimal)(firstCharLast.High.GetValueOrDefault() * (double)tickSize));
        firstResult.Low.Should().Be(open + (decimal)(firstCharLast.Low.GetValueOrDefault() * (double)tickSize));
        firstResult.Close.Should().Be(open + (decimal)(firstCharLast.Close.GetValueOrDefault() * (double)tickSize));
        firstResult.Volume.Should().Be((decimal)firstCharLast.Vol.GetValueOrDefault());
        firstResult.Date.Should().Be(dateStart);

        result.Last().Date.Should().Be(dateEnd);
    }

    [Fact]
    public async Task Test_GetChart_bydate_Error()
    {
        // Arrange 
        var symbolresponsedata = TestUtils.FileReadContent(RessourcePath, "symbolInfo.json");
        var symbolresponse = new SymbolResponse(symbolresponsedata);
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(symbolresponse);

        _apiCommandFactoryXtb.Setup(x => x.ExecuteChartRangeCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(),
                It.IsAny<PERIOD_CODE>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // act
        var dateStart = new DateTime(2022, 11, 24, 20, 25, 00);
        var dateEnd = new DateTime(2022, 11, 25, 20, 55, 00);

        Func<Task<List<Candle>>> result = async () =>
            await _xtbApi.GetChartByDateAsync("EURUSD", Timeframe.OneMinute, dateStart, dateEnd);

        // Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Test_GetChart_bydate_Error_symbol()
    {
        // Arrange 
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new Exception());


        // act
        var dateStart = new DateTime(2022, 11, 24, 20, 25, 00);
        var dateEnd = new DateTime(2022, 11, 25, 20, 55, 00);

        Func<Task<List<Candle>>> result = async () =>
            await _xtbApi.GetChartByDateAsync("EURUSD", Timeframe.OneMinute, dateStart, dateEnd);

        // Assert
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Get tick price

    [Fact]
    public async Task Test_GetTickPrice_Success()
    {
        // Arrange
        var tickPriceData = TestUtils.FileReadContent(RessourcePath, "tickPrice.json");
        var tickPricesResponse = new TickPricesResponse(tickPriceData);
        _apiCommandFactoryXtb.Setup(x => x.ExecuteTickPricesCommand(It.IsAny<ISyncApiConnector>(),
                It.IsAny<List<string>>(), It.IsAny<long?>(), It.IsAny<bool>()))
            .Returns(tickPricesResponse);

        // Act
        var result = await _xtbApi.GetTickPriceAsync("test");

        // Assert 
        result.Ask.Should().Be(100);
        result.Bid.Should().Be(101);
        result.Symbol.Should().Be("test");
    }

    [Fact]
    public async Task Test_GetTickPrice_error()
    {
        // Arrange
        _apiCommandFactoryXtb.Setup(x => x.ExecuteTickPricesCommand(It.IsAny<ISyncApiConnector>(),
                It.IsAny<List<string>>(), It.IsAny<long?>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        Func<Task<Tick>> result = async () => await _xtbApi.GetTickPriceAsync("test");

        // Assert 
        await result.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region OpenPosition

    [Fact]
    public async Task Test_OpenPosition()
    {
        // Arrange
        var tradeTransactionRsp = TestUtils.FileReadContent(RessourcePath, "transactionResponse.json");
        var tradeTransactionResponse = new TradeTransactionResponse(tradeTransactionRsp);

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Returns(tradeTransactionResponse);

        // Act
        var positionRsp =
            await _xtbApi.OpenPositionAsync(new Position()
                .SetId("test")
                .SetCustomComment("test")
                .SetOpenPrice(1)
                .SetStopLoss(1)
                .SetTakeProfit(1)
                .SetVolume(1)
                .SetOrder(1)
                .SetSymbol("test")
                .SetStatusPosition(StatusPosition.Pending)
            );

        // Assert
        _apiCommandFactoryXtb.Verify(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(),
                It.Is<TradeTransInfoRecord>(
                    record =>
                        record.Cmd == TRADE_OPERATION_CODE.BUY &&
                        record.Type == TRADE_TRANSACTION_TYPE.ORDER_OPEN &&
                        record.Price == 1 &&
                        record.Sl == 1 &&
                        record.Tp == 1 &&
                        record.Volume == 1 &&
                        record.CustomComment == "test" &&
                        record.Symbol == "test" &&
                        record.Order == 0),
                It.IsAny<bool>()), Times.Exactly(1));

        positionRsp.Order.Should().Be(tradeTransactionResponse.Order);
        positionRsp.Order2.Should().Be(tradeTransactionResponse.Order);
        positionRsp.PositionId.Should().Be(tradeTransactionResponse.Order);
        positionRsp.StatusPosition.Should().Be(StatusPosition.Pending);
        _xtbApi.CachePosition.Should().Contain(positionRsp);
    }

    [Fact]
    public async Task Test_OpenPosition_error()
    {
        // Arrange

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Throws(new Exception());

        // Act
        Func<Task<Position>> positionRsp = async () =>
            await _xtbApi.OpenPositionAsync(new Position()
                .SetId("test")
                .SetCustomComment("test")
                .SetOpenPrice(1)
                .SetStopLoss(1)
                .SetTakeProfit(1)
                .SetVolume(1));

        // Assert
        await positionRsp.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Exactly(1));
    }

    [Fact]
    public async Task test_positionOpenedCallback_bad_orderRef()
    {
        // Arrange
        await Test_OpenPosition();

        var caller = false;

        var streamingTradeRecord = new StreamingTradeRecord();
        streamingTradeRecord.Order = 10;
        streamingTradeRecord.Order2 = 10;
        streamingTradeRecord.Sl = 10;
        streamingTradeRecord.Tp = 10;
        streamingTradeRecord.Position = 10;
        streamingTradeRecord.Comment = "testComment";
        streamingTradeRecord.CustomComment = "test";
        streamingTradeRecord.Type = STREAMING_TRADE_TYPE.OPEN;
        streamingTradeRecord.Cmd = 0;
        streamingTradeRecord.Open_time = 1669491977989;

        _xtbApi.PositionOpenedEvent += (_, _) => { caller = true; };

        // Act
        _streamingApiConnector.Raise(x => x.TradeRecordReceived += null, streamingTradeRecord);

        // Assert
        caller.Should().BeFalse();
    }

    /// <summary>
    ///     Utiliser pour ouvrir position dans d'autres tests
    /// </summary>
    [Fact]
    public async Task test_positionOpenedCallback_good_orderRef()
    {
        // Arrange
        ConnectMock();
        await Test_OpenPosition();

        var caller = false;

        var streamingTradeRecord = new StreamingTradeRecord();
        streamingTradeRecord.Order = 10;
        streamingTradeRecord.Order2 = 20;
        streamingTradeRecord.Sl = 10;
        streamingTradeRecord.Tp = 10;
        streamingTradeRecord.Open_price = 10;
        streamingTradeRecord.Position = 10;
        streamingTradeRecord.Comment = "testComment";
        streamingTradeRecord.CustomComment = "test";
        streamingTradeRecord.Type = STREAMING_TRADE_TYPE.OPEN;
        streamingTradeRecord.Cmd = 0;
        streamingTradeRecord.Open_time = 1669491977989;
        streamingTradeRecord.Volume = 10;

        _xtbApi.PositionOpenedEvent += (_, position) =>
        {
            // Assert tests
            caller = true;
            position.Order.Should().Be(streamingTradeRecord.Order);
            position.Order2.Should().Be(streamingTradeRecord.Order2);
            position.PositionId.Should().Be(streamingTradeRecord.Position);
            position.StatusPosition.Should().Be(StatusPosition.Open);
            position.StopLoss.Should().Be(10);
            position.TakeProfit.Should().Be(10);
            position.OpenPrice.Should().Be(10);
            position.DateOpen.Should().Be(streamingTradeRecord.Open_time.GetValueOrDefault().ConvertToDatetime());
        };

        // Act
        _streamingApiConnector.Raise(x => x.TradeRecordReceived += null, streamingTradeRecord);

        // Assert
        caller.Should().BeTrue();
    }

    #endregion

    #region Update position

    [Fact]
    public async Task Test_UpdatePosition_Success()
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();


        var tradeTransactionRsp = TestUtils.FileReadContent(RessourcePath, "transactionResponse.json");
        var tradeTransactionResponse = new TradeTransactionResponse(tradeTransactionRsp);

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Returns(tradeTransactionResponse);


        var positionToUpdate = new Position()
            .SetSymbol("test")
            .SetCustomComment("test")
            .SetId("test")
            .SetTypePosition(TypePosition.Buy)
            .SetOrder(1)
            .SetTakeProfit(2)
            .SetStopLoss(2)
            .SetVolume(0);

        // Act

        await _xtbApi.UpdatePositionAsync(1, positionToUpdate);

        // Assert

        _apiCommandFactoryXtb.Verify(x => x.ExecuteTradeTransactionCommand(
            It.IsAny<ISyncApiConnector>(),
            It.Is<TradeTransInfoRecord>(
                record =>
                    record.Cmd == TRADE_OPERATION_CODE.BUY &&
                    record.Type == TRADE_TRANSACTION_TYPE.ORDER_MODIFY &&
                    record.Price == 1 &&
                    record.Sl == 10 &&
                    record.Tp == 10 &&
                    record.Symbol == "test" &&
                    record.Volume == 10 &&
                    record.Order == 10
            ),
            It.IsAny<bool>()));
    }

    [Fact]
    public async Task Test_UpdatePosition_Error()
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Throws(new Exception());


        // Act

        var act = async () => await _xtbApi.UpdatePositionAsync(1, new Position { Id = "test" });

        // Assert
        await act.Should().ThrowAsync<ApiHandlerException>();
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<long?>()));
    }

    [Fact]
    public async Task Test_UpdatePosition_Warning_no_id()
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Throws(new Exception());


        // Act

        await _xtbApi.UpdatePositionAsync(1, new Position());

        // Assert

        _logger.Verify(x => x.Warning(It.IsAny<string>(), It.IsAny<string>()));
    }

    [Theory]
    [InlineData(StatusPosition.Close)]
    [InlineData(StatusPosition.WaitClose)]
    public async Task Test_UpdatePosition_warning_status(StatusPosition statusPosition)
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();


        var tradeTransactionRsp = TestUtils.FileReadContent(RessourcePath, "transactionResponse.json");
        var tradeTransactionResponse = new TradeTransactionResponse(tradeTransactionRsp);

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Returns(tradeTransactionResponse);


        var positionToUpdate = new Position()
            .SetSymbol("test")
            .SetCustomComment("test")
            .SetId("test")
            .SetStatusPosition(statusPosition);

        // Act

        await _xtbApi.UpdatePositionAsync(1, positionToUpdate);

        // Assert

        _apiCommandFactoryXtb.Verify(x => x.ExecuteTradeTransactionCommand(
            It.IsAny<ISyncApiConnector>(),
            It.Is<TradeTransInfoRecord>(tradeTransInfoRecord =>
                tradeTransInfoRecord.Type == TRADE_TRANSACTION_TYPE.ORDER_MODIFY),
            It.IsAny<bool>()), Times.Never);
        _logger.Verify(x => x.Warning(It.IsAny<string>(), It.IsAny<StatusPosition>()));
    }

    [Fact]
    public async Task test_UpdatePositionCallback_sl_tp()
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();

        var caller = false;

        var streamingTradeRecord = new StreamingTradeRecord();
        streamingTradeRecord.Sl = 100;
        streamingTradeRecord.Tp = 200;
        streamingTradeRecord.Order = 10;
        streamingTradeRecord.Order2 = 20;
        streamingTradeRecord.Position = 10;
        streamingTradeRecord.Comment = "testComment";
        streamingTradeRecord.CustomComment = "testCustomComment";
        streamingTradeRecord.Type = STREAMING_TRADE_TYPE.OPEN;
        streamingTradeRecord.Cmd = 0;
        streamingTradeRecord.Open_time = 1669491977989;

        _xtbApi.PositionUpdatedEvent += (_, position) =>
        {
            // Assert event
            caller = true;
            position.StopLoss.Should().Be(100);
            position.TakeProfit.Should().Be(200);
        };

        // Act
        _streamingApiConnector.Raise(x => x.TradeRecordReceived += null, streamingTradeRecord);

        // Assert
        caller.Should().BeTrue();
    }

    #endregion

    #region Close position

    [Fact]
    public async Task Test_ClosePosition_Success()
    {
        // Arrange
        ConnectMock();
        await test_positionOpenedCallback_good_orderRef();


        var tradeTransactionRsp = TestUtils.FileReadContent(RessourcePath, "transactionResponse.json");
        var tradeTransactionResponse = new TradeTransactionResponse(tradeTransactionRsp);

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Returns(tradeTransactionResponse);


        var positionToClose = new Position()
            .SetSymbol("test")
            .SetCustomComment("test")
            .SetId("test");


        // Act

        await _xtbApi.ClosePositionAsync(1, positionToClose);

        // Assert

        _apiCommandFactoryXtb.Verify(x => x.ExecuteTradeTransactionCommand(
            It.IsAny<ISyncApiConnector>(),
            It.Is<TradeTransInfoRecord>(
                record =>
                    record.Cmd == TRADE_OPERATION_CODE.BUY &&
                    record.Type == TRADE_TRANSACTION_TYPE.ORDER_CLOSE &&
                    record.Price == 1 &&
                    record.Sl == 10 &&
                    record.Tp == 10 &&
                    record.Symbol == "test" &&
                    record.Volume == 10 &&
                    record.Order == 10
            ),
            It.IsAny<bool>()));
    }

    [Fact]
    public async Task Test_ClosePosition_Error()
    {
        // Arrange
        ConnectMock();
        await test_positionOpenedCallback_good_orderRef();

        _apiCommandFactoryXtb.Setup(x =>
            x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<TradeTransInfoRecord>(),
                It.IsAny<bool>())).Throws(new Exception());


        // Act

        var act = async () =>
            await _xtbApi.ClosePositionAsync(1, new Position { StatusPosition = StatusPosition.Open, Id = "test" });

        // Assert

        await act.Should().ThrowAsync<ApiHandlerException>();
    }

    [Fact]
    public async Task Test_ClosePosition_Warning_noSelected()
    {
        // Arrange
        ConnectMock();
        await test_positionOpenedCallback_good_orderRef();


        // Act

        await _xtbApi.ClosePositionAsync(1, new Position { StatusPosition = StatusPosition.Open, Id = "truc" });

        // Assert

        _apiCommandFactoryXtb.Verify(
            x => x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(),
                It.Is<TradeTransInfoRecord>(x => x.Type == TRADE_TRANSACTION_TYPE.ORDER_DELETE), It.IsAny<bool>()),
            Times.Never);
        _logger.Verify(x => x.Warning(It.IsAny<string>(), It.IsAny<string>()));
    }

    [Fact]
    public async Task Test_ClosePosition_Warning_State()
    {
        // Arrange
        ConnectMock();
        await test_positionOpenedCallback_good_orderRef();


        // Act

        await _xtbApi.ClosePositionAsync(1, new Position { StatusPosition = StatusPosition.Close, Id = "test" });

        // Assert

        _apiCommandFactoryXtb.Verify(
            x => x.ExecuteTradeTransactionCommand(It.IsAny<ISyncApiConnector>(),
                It.Is<TradeTransInfoRecord>(x => x.Type == TRADE_TRANSACTION_TYPE.ORDER_DELETE), It.IsAny<bool>()),
            Times.Never);
        _logger.Verify(x => x.Warning(It.IsAny<string>(), It.IsAny<StatusPosition>()), Times.Exactly(1));
    }


    [Fact]
    public async Task test_ClosePositionCallback()
    {
        // Arrange
        ConnectMock();
        await test_positionOpenedCallback_good_orderRef();

        var caller = false;

        var streamingTradeRecord = new StreamingTradeRecord();
        streamingTradeRecord.Sl = 10;
        streamingTradeRecord.Tp = 10;
        streamingTradeRecord.Order = 10;
        streamingTradeRecord.Order2 = 20;
        streamingTradeRecord.Position = 10;
        streamingTradeRecord.Comment = "testComment";
        streamingTradeRecord.CustomComment = "testCustomComment";
        streamingTradeRecord.Type = STREAMING_TRADE_TYPE.CLOSE;
        streamingTradeRecord.Cmd = 0;
        streamingTradeRecord.Open_time = 1669491977989;
        streamingTradeRecord.Close_time = 1669491977989;
        streamingTradeRecord.Close_price = 10;
        streamingTradeRecord.Profit = 20;
        streamingTradeRecord.Closed = true;

        _xtbApi.PositionClosedEvent += (_, position) =>
        {
            // Assert event
            caller = true;
            position.StatusPosition.Should().Be(StatusPosition.Close);
            position.DateClose.Should().Be(streamingTradeRecord.Close_time.GetValueOrDefault().ConvertToDatetime());
            position.ClosePrice.Should().Be(10);
            position.Profit.Should().Be(20);
        };

        // Act
        _streamingApiConnector.Raise(x => x.TradeRecordReceived += null, streamingTradeRecord);

        // Assert
        caller.Should().BeTrue();
    }

    #endregion

    #region Check if symbol exist

    [Fact]
    public async Task Test_Check_Symbol_Exist_Success()
    {
        // Arrange
        var symbolresponsedata = TestUtils.FileReadContent(RessourcePath, "symbolInfo.json");
        var symbolresponse = new SymbolResponse(symbolresponsedata);
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(symbolresponse);

        // Act
        var result = await _xtbApi.CheckIfSymbolExistAsync("test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Test_Check_Symbol_Exist_False()
    {
        // Arrange
        _apiCommandFactoryXtb.Setup(x =>
                x.ExecuteSymbolCommand(It.IsAny<ISyncApiConnector>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new Exception());

        // Act
        var result = await _xtbApi.CheckIfSymbolExistAsync("test");

        // Assert
        result.Should().BeFalse();
    }

    #endregion


    #region TickRecord

    [Fact]
    public void test_tickRecord_level0()
    {
        // Arrange
        ConnectMock();
        var caller = false;
        var streamingTickRecord = new StreamingTickRecord();
        streamingTickRecord.Level = 0;
        streamingTickRecord.Symbol = "test";
        streamingTickRecord.Ask = 10;
        streamingTickRecord.Bid = 10;

        _xtbApi.TickEvent += (_, tick) =>
        {
            // Assert event
            caller = true;
            tick.Ask.Should().Be(10);
            tick.Bid.Should().Be(10);
        };

        // Act
        _streamingApiConnector.Raise(x => x.TickRecordReceived += null, streamingTickRecord);

        // Assert
        caller.Should().BeTrue();
    }

    [Fact]
    public async void test_tickRecord_levelnot0()
    {
        // Arrange
        ConnectMock();
        var caller = false;
        var streamingTickRecord = new StreamingTickRecord();
        streamingTickRecord.Level = 1;

        _xtbApi.TickEvent += (_, _) => { caller = true; };

        // Act
        _streamingApiConnector.Raise(x => x.TickRecordReceived += null, streamingTickRecord);

        // Assert
        caller.Should().BeFalse();
    }

    #endregion

    #region PositionRejected

    [Fact]
    public async Task test_PositionRejected()
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();

        var caller = false;
        var streamingTradeStatusRecord = new StreamingTradeStatusRecord();
        streamingTradeStatusRecord.RequestStatus = REQUEST_STATUS.REJECTED;
        streamingTradeStatusRecord.Order = 20;

        _xtbApi.PositionRejectedEvent += (_, position) =>
        {
            caller = true;
            position.StatusPosition.Should().Be(StatusPosition.Rejected);
        };

        // Act
        _streamingApiConnector.Raise(x => x.TradeStatusRecordReceived += null, streamingTradeStatusRecord);

        // Assert
        caller.Should().BeTrue();
    }

    [Fact]
    public async Task test_PositionRejected_noselected()
    {
        // Arrange
        await test_positionOpenedCallback_good_orderRef();

        var caller = false;
        var streamingTradeStatusRecord = new StreamingTradeStatusRecord();
        streamingTradeStatusRecord.RequestStatus = REQUEST_STATUS.REJECTED;
        streamingTradeStatusRecord.Order = 200;

        _xtbApi.PositionRejectedEvent += (_, _) => { caller = true; };

        // Act
        _streamingApiConnector.Raise(x => x.TradeStatusRecordReceived += null, streamingTradeStatusRecord);

        // Assert
        caller.Should().BeFalse();
    }

    #endregion
}