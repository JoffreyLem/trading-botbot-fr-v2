using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;
using RobotAppLibraryV2.ApiHandler.Exception;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.ApiHandler.Xtb;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;
using RobotAppLibraryV2.ApiHandler.Xtb.records;
using RobotAppLibraryV2.ApiHandler.Xtb.sync;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;
using Serilog;
using Serilog.Context;
using Timer = System.Timers.Timer;

namespace RobotAppLibraryV2.ApiHandler.Handlers;

public sealed class XtbApi : IApiHandler, IDisposable
{
    private const string PositionIdContext = "PositionIdContext";
    private readonly IApiCommandFactory _apiCommandExecutor;

    private readonly List<Position> _cachePosition = new();
    private readonly ILogger _logger;
    private readonly Timer _timer = new();

    public XtbApi(IApiCommandFactory apiCommandExecutor, ISyncApiConnector connector, ILogger logger)
    {
        Connector = connector;
        _apiCommandExecutor = apiCommandExecutor;
        _logger = logger.ForContext<XtbApi>();

        Connector.OnConnected += server =>
            _logger.Information("Xtb connector connected to server {Server}:{Port}", server.Address, server.MainPort);
        Connector.OnDisconnectedCallBack += () =>
        {
            // TODO : TU ici
            _logger.Information("Xtb connector disconnected");
            Disconnected?.Invoke(this, EventArgs.Empty);
        };
        Connector.OnRedirected += server =>
            _logger.Information("Xtb connector redirected to server {Server}:{Port}", server.Address, server.MainPort);
    }

    private ISyncApiConnector Connector { get; }
    public IReadOnlyList<Position> CachePosition => _cachePosition.AsReadOnly();

    public byte[] SymbolsCompressed { get; set; } 
    public AccountBalance AccountBalance { get; set; } = new();
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler<Tick>? TickEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;
    public event EventHandler<AccountBalance>? NewBalanceEvent;
    public event EventHandler<News>? NewsEvent;

    public Task ConnectAsync(string user, string pwd)
    {
        try
        {
            _logger.Information("Trying to connect to XTB API");
            Connector.Connect();
            var credentials = new Credentials(user, pwd);
            _apiCommandExecutor.ExecuteLoginCommand(Connector, credentials, true);
            Connector.StreamingApiConnector.Connect();
            Connector.StreamingApiConnector.OnConnected += server =>
                _logger.Information("Xtb streaming connector connected to server {Server}:{Port}", server.Address,
                    server.MainPort);
            Connector.StreamingApiConnector.OnDisconnectedStreamingCallback +=
                () => _logger.Information("Xtb streaming connector disconnected");

            Connector.StreamingApiConnector.BalanceRecordReceived += StreamingApiConnectorOnBalanceRecordReceived;
            Connector.StreamingApiConnector.NewsRecordReceived += StreamingApiConnectorOnNewsRecordReceived;
            Connector.StreamingApiConnector.TickRecordReceived += StreamingApiConnectorOnTickRecordReceived;
            Connector.StreamingApiConnector.ProfitRecordReceived += StreamingApiConnectorOnProfitRecordReceived;
            Connector.StreamingApiConnector.TradeStatusRecordReceived +=
                StreamingApiConnectorOnTradeStatusRecordReceived;
            Connector.StreamingApiConnector.TradeRecordReceived += StreamingApiConnectorOnTradeRecordReceived;
            GetSymbolsInternal();
            EnableStreaming();
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            _timer.Elapsed += async (_, _) => await PingHandlerAsync();
            _logger.Information("Connected to XTB API");
            return Task.CompletedTask;
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Error on connect");
            throw new ApiHandlerException();
        }
    }


    public async Task DisconnectAsync()
    {
        try
        {
            _timer.Enabled = false;
            DisableStreaming();
            TickEvent = null;
            PositionOpenedEvent = null;
            PositionUpdatedEvent = null;
            PositionRejectedEvent = null;
            PositionClosedEvent = null;
            NewBalanceEvent = null;

            var positions = await GetCurrentTradesAsync();

            foreach (var positionModele in positions.Where(x => !string.IsNullOrEmpty(x.CustomComment)))
                try
                {
                    var price = await GetTickPriceAsync(positionModele.Symbol);
                    await ClosePositionAsync(price.Bid.GetValueOrDefault(), positionModele);
                }
                catch (System.Exception e)
                {
                    _logger.Error(e, "Can't close position {Position}", positionModele.Id);
                }

            Connector.StreamingApiConnector.Disconnect();
            Connector.Disconnect();
            Dispose();
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement {Method}", nameof(DisconnectAsync));
            throw new ApiHandlerException();
        }
    }

    [ExcludeFromCodeCoverage]
    public bool IsConnected()
    {
        return Connector.Connected();
    }

    public Task PingAsync()
    {
        try
        {
            _apiCommandExecutor.ExecutePingCommand(Connector);
            _logger.Information("Ping pong");
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Error on Ping");
        }

        return Task.CompletedTask;
    }

    public Task<AccountBalance> GetBalanceAsync()
    {
        try
        {
            var data = _apiCommandExecutor.ExecuteMarginLevelCommand(Connector);
            var balanceModele = new AccountBalance
            {
                Balance = data.Balance.GetValueOrDefault(),
                Credit = data.Credit,
                Equity = data.Equity,
                Margin = data.Margin,
                MarginFree = data.Margin_free,
                MarginLevel = data.Margin_level
            };
            AccountBalance = balanceModele;
            return Task.FromResult(balanceModele);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Error on Get balance");
            throw new ApiHandlerException();
        }
    }

    public Task<List<Position>> GetAllPositionsAsync()
    {
        try
        {
            var data = _apiCommandExecutor.ExecuteTradesHistoryCommand(Connector,
                new DateTime(2023, 01, 01, 0, 0, 0, DateTimeKind.Utc).ConvertToUnixTime(),
                DateTime.Now.ConvertToUnixTime());

            var rsp =
                MapTradeRecordsToPositions(data.TradeRecords.Where(x => x.Closed.GetValueOrDefault()).ToList(),
                    StatusPosition.Close);

            return Task.FromResult(rsp);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement GetAllPositions");
            throw new ApiHandlerException("Erreur traitement GetAllPositions", e);
        }
    }

    public Task<List<Calendar>> GetCalendarAsync()
    {
        try
        {
            var calendarData = _apiCommandExecutor.ExecuteCalendarCommand(Connector);

            var dataToRetun = calendarData.CalendarRecords.Select(x => new Calendar
            {
                Country = x.Country,
                Current = x.Current,
                Forecast = x.Forecast,
                Impact = x.Impact,
                Period = x.Period,
                Previous = x.Previous,
                Time = x.Time.GetValueOrDefault().ConvertToDatetime(),
                Title = x.Title
            }).ToList();

            return Task.FromResult(dataToRetun);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement GetCalendar");
            throw new ApiHandlerException("Erreur traitement GetCalendar", e);
        }
    }

    //TODO : Changer pour tout retourner
    private void GetSymbolsInternal()
    {
        var alls = _apiCommandExecutor.ExecuteAllSymbolsCommand(Connector);
        var listData = new List<string>(alls.SymbolRecords.Count);
        
        foreach (var allsSymbolRecord in alls.SymbolRecords)
        {
            listData.Add(allsSymbolRecord.Symbol);
        }
        using var memoryStream = new MemoryStream();
        using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
        var jsonData = System.Text.Json.JsonSerializer.Serialize(listData);
        var byteArray = Encoding.UTF8.GetBytes(jsonData);
        gZipStream.Write(byteArray, 0, byteArray.Length);
        gZipStream.Close();
        SymbolsCompressed = memoryStream.ToArray();
        listData.Clear();
        listData = null;
    }

    public Task<List<string>?> GetAllSymbolsAsync()
    {
        try
        {
            using var compressedMemoryStream = new MemoryStream(SymbolsCompressed);
            using var decompressedMemoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress);
            gZipStream.CopyTo(decompressedMemoryStream);
            decompressedMemoryStream.Position = 0; 
            var jsonData = Encoding.UTF8.GetString(decompressedMemoryStream.ToArray());
            var rsp = System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonData);
            
            return Task.FromResult(rsp);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement Get all symbols");
            throw new ApiHandlerException("Erreur traitement Get all symbols", e);
        }
    }

    public Task<List<Position>> GetCurrentTradesAsync()
    {
        try
        {
            var data = _apiCommandExecutor.ExecuteTradesCommand(Connector, true);
            var dataToReturn = MapTradeRecordsToPositions(data.TradeRecords.ToList(), StatusPosition.Open);
            _logger.Information("Current trades response {@Trades}", dataToReturn);
            return Task.FromResult(dataToReturn);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement Get current trades");
            throw new ApiHandlerException("Erreur traitement GetCurrentTrades", e);
        }
    }

    public async Task<List<Position>> GetAllPositionsByCommentAsync(string comment)
    {
        try
        {
            return (await GetAllPositionsAsync()).Where(x => x.CustomComment == comment).ToList();
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement GetAllPositions");
            throw new ApiHandlerException("Erreur traitement GetAllPositions", e);
        }
    }

    public Task<SymbolInfo> GetSymbolInformationAsync(string symbol)
    {
        try
        {
            var data = _apiCommandExecutor.ExecuteSymbolCommand(Connector, symbol);
            var x = data.Symbol;
            var symbolToReturn = new SymbolInfo()
                .WithCategory(GetCategory(x.CategoryName))
                .WithContractSize(x.ContractSize)
                .WithCurrency1(x.Currency)
                .WithCurrency2(x.CurrencyProfit)
                .WithLotMax(x.LotMax)
                .WithLotMin(x.LotMin)
                .WithPrecision(x.Precision)
                .WithSymbol(x.Symbol)
                .WithTickValue(x.TickValue.GetValueOrDefault())
                .WithTickSize(x.TickSize.GetValueOrDefault());

            if (symbolToReturn.Category == Category.Forex)
                symbolToReturn.WithTickSize2(x.TickValue.GetValueOrDefault() / 10);

            return Task.FromResult(symbolToReturn);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Error on get symbol information for symbol {Symbol}", symbol);
            throw new ApiHandlerException($"Symbol {symbol} not found or error", e);
        }
    }

    public Task<TradeHourRecord> GetTradingHoursAsync(string symbol)
    {
        try
        {
            var tradeHour =
                _apiCommandExecutor.ExecuteTradingHoursCommand(Connector, new List<string> { symbol });

            var selected = tradeHour.TradingHoursRecords.First?.ValueRef;

            var tradeHoursRecords = new TradeHourRecord();

            var data = selected?.Trading.OrderBy(x => x.Day).ToList();

            if (data is null) throw new ApiHandlerException("No trading hours");

            foreach (var hoursRecord in data)
            {
                var hourRecord = new TradeHourRecord.HoursRecordData();

                hourRecord.Day = Utils.Utils.GetDay(hoursRecord.Day.GetValueOrDefault());
                hourRecord.From = TimeSpan.FromMilliseconds(hoursRecord.FromT.GetValueOrDefault());
                hourRecord.To = TimeSpan.FromMilliseconds(hoursRecord.ToT.GetValueOrDefault());

                tradeHoursRecords.HoursRecords.Add(hourRecord);
            }

            return Task.FromResult(tradeHoursRecords);
        }
        catch (System.Exception e) when (e is not ApiHandlerException)
        {
            _logger.Error(e, "Error on get trading hours information for symbol {Symbol}", symbol);
            throw new ApiHandlerException($"Symbol {symbol} not found", e);
        }
    }

    public async Task<List<Candle>> GetChartAsync(string symbol, Timeframe timeframe)
    {
        try
        {
            var data = SetDateTime(timeframe);

            var selectedSymbol = await GetSymbolInformationAsync(symbol);

            var chartLastResponse = _apiCommandExecutor.ExecuteChartLastCommand(Connector, symbol,
                data.periodCode,
                data.dateTime.ConvertToUnixTime());
            var rsp = ConvertChartResponseToCandlesList(chartLastResponse.RateInfos,
                selectedSymbol.TickSize);
            return rsp;
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement get chart");
            throw new ApiHandlerException($"Erreur traitement {nameof(GetChartAsync)}", e);
        }
    }

    public async Task<List<Candle>> GetChartByDateAsync(string symbol, Timeframe periodCodeStr, DateTime start,
        DateTime end)
    {
        try
        {
            var selectedSymbol = await GetSymbolInformationAsync(symbol);
            var data = SetDateTime(periodCodeStr);

            var chartRangeResponse = _apiCommandExecutor.ExecuteChartRangeCommand(Connector,
                selectedSymbol.Symbol ?? throw new InvalidOperationException("Selected symbol is null"),
                data.periodCode,
                start.ConvertToUnixTime(), end.ConvertToUnixTime(), 0);
            var rsp = ConvertChartResponseToCandlesList(chartRangeResponse.RateInfos,
                selectedSymbol.TickSize);
            return rsp;
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement get chart by date");
            throw new ApiHandlerException($"Erreur traitement {nameof(GetChartByDateAsync)}", e);
        }
    }

    public Task<Tick> GetTickPriceAsync(string symbol)
    {
        try
        {
            var symbolData = new List<string> { symbol };

            var data = _apiCommandExecutor.ExecuteTickPricesCommand(Connector, symbolData,
                0);

            var ask = (decimal?)data.Ticks.First?.Value.Ask;
            var bid = (decimal?)data.Ticks.First?.Value.Bid;
            var date = data.Ticks.First?.Value.Timestamp.GetValueOrDefault().ConvertToDatetime();
            var tick = new Tick(ask, bid, date.GetValueOrDefault(), symbol);
            return Task.FromResult(tick);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement get tick price pour {Symbol}", symbol);
            throw new ApiHandlerException($"Erreur traitement {nameof(GetTickPriceAsync)}", e);
        }
    }

    public Task<Position> OpenPositionAsync(Position position)
    {
        try
        {
            using (LogContext.PushProperty(PositionIdContext, position.Id))
            {
                var ttOpenInfoRecord = new TradeTransInfoRecord(
                    GetTradeOperationByTypePosition(position.TypePosition),
                    TRADE_TRANSACTION_TYPE.ORDER_OPEN,
                    (double?)position.OpenPrice, (double?)position.StopLoss, (double?)position.TakeProfit,
                    position.Symbol,
                    position.Volume, 0, position.CustomComment, 0);
                _logger.Information("Try Opening Position {@Position}", position);
                _cachePosition.Add(position);
                var rsp =
                    _apiCommandExecutor.ExecuteTradeTransactionCommand(Connector, ttOpenInfoRecord);
                _logger.Information("Order open accepted for {@Position}", new { rsp, position.Id });
                position.Order = rsp.Order;
                position.Order2 = rsp.Order;
                position.PositionId = rsp.Order;
                position.StatusPosition = StatusPosition.Pending;
                return Task.FromResult(position);
            }
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Can't open position {Position}", position.PositionId);
            throw new ApiHandlerException($"Can't open position {position.PositionId}", e);
        }
    }

    public Task UpdatePositionAsync(decimal price, Position position)
    {
        try
        {
            using (LogContext.PushProperty(PositionIdContext, position.Id))
            {
                if (position.StatusPosition is StatusPosition.WaitClose or StatusPosition.Close)
                {
                    _logger.Warning("Can't update position beceause status is {Status}", position.StatusPosition);
                    return Task.CompletedTask;
                }

                var selected = _cachePosition.Find(x => x.Id == position.Id);
                if (selected is null)
                {
                    _logger.Warning("Can't update position {Id} because unmanaged", position.Id);
                    return Task.CompletedTask;
                }

                _logger.Information("Trying update position {@Position}", position);
                var type = GetOperationCode((int)position.TypePosition);

                var tradeTransInfoRecord = new TradeTransInfoRecord(type,
                    TRADE_TRANSACTION_TYPE.ORDER_MODIFY, (double?)price, (double?)selected.StopLoss,
                    (double?)selected.TakeProfit,
                    selected.Symbol, selected.Volume, selected.Order, selected.CustomComment, 0);

                var rsp = _apiCommandExecutor.ExecuteTradeTransactionCommand(Connector,
                    tradeTransInfoRecord);
                _logger.Information("Order update accepted for {@Position}", new { rsp, position.Id });
            }

            return Task.CompletedTask;
        }

        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur update position {Id}", position.PositionId);
            throw new ApiHandlerException($"Erreur traitement {nameof(UpdatePositionAsync)}", e);
        }
    }

    public Task ClosePositionAsync(decimal price, Position position)
    {
        try
        {
            using (LogContext.PushProperty(PositionIdContext, position.Id))
            {
                var selected = _cachePosition.Find(x =>
                    x.Id == position.Id ||
                    (x.Order != null && x.Order == position.Order) ||
                    (x.Order2 != null && x.Order2 == position.Order2) ||
                    (x.PositionId != null && x.PositionId == position.PositionId)
                );

                if (selected is null)
                {
                    _logger.Warning("Can't close position {Id} because unmanaged", position.Id);
                    return Task.CompletedTask;
                }

                if (position.StatusPosition is StatusPosition.Open or StatusPosition.WaitClose)
                {
                    _logger.Information("Trying close position {@Position}", position);
                    var sl = selected.StopLoss;
                    var tp = selected.TakeProfit;
                    var symbol = selected.Symbol;
                    double? volume = selected.Volume;
                    var order = selected.Order.GetValueOrDefault();
                    var customComment = selected.CustomComment;
                    var expiration = 0;
                    var typePosition =
                        GetOperationCode((int)position.TypePosition);
                    var ttCloseInfoRecord = new TradeTransInfoRecord(
                        typePosition,
                        TRADE_TRANSACTION_TYPE.ORDER_CLOSE,
                        (double?)price, (double?)sl, (double?)tp, symbol, volume, order, customComment, expiration);

                    var rsp =
                        _apiCommandExecutor.ExecuteTradeTransactionCommand(Connector, ttCloseInfoRecord, true);
                    _logger.Information("Order close accepted for {@Position}", new { rsp, position.Id });
                }
                else
                {
                    _logger.Warning("Can't close position beceause status is {Status}", position.StatusPosition);
                }

                return Task.CompletedTask;
            }
        }

        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur close position ");
            throw new ApiHandlerException($"Erreur traitement {nameof(ClosePositionAsync)}", e);
        }
    }

    public async Task<bool> CheckIfSymbolExistAsync(string symbol)
    {
        try
        {
            await GetSymbolInformationAsync(symbol);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public void SubscribePrice(string symbol)
    {
        Connector.StreamingApiConnector.SubscribePrice(symbol);
    }

    public void UnsubscribePrice(string symbol)
    {
        Connector.StreamingApiConnector.UnsubscribePrice(symbol);
    }


    public void Dispose()
    {
        _timer.Dispose();
    }

    private void EnableStreaming()
    {
        try
        {
            _logger.Information("Enable streaming");
            Connector.StreamingApiConnector.SubscribeKeepAlive();
            Connector.StreamingApiConnector.SubscribeBalance();
            Connector.StreamingApiConnector.SubscribeTrades();
            Connector.StreamingApiConnector.SubscribeTradeStatus();
            Connector.StreamingApiConnector.SubscribeProfits();
            Connector.StreamingApiConnector.SubscribeNews();
            _logger.Information("Streaming enabled");
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Error on enable streaming");
            throw;
        }
    }

    private void DisableStreaming()
    {
        try
        {
            _logger.Information("Disable streaming");
            Connector.StreamingApiConnector.UnsubscribeBalance();
            Connector.StreamingApiConnector.UnsubscribeTrades();
            Connector.StreamingApiConnector.UnsubscribeTradeStatus();
            Connector.StreamingApiConnector.UnsubscribeProfits();
            Connector.StreamingApiConnector.UnsubscribeNews();
            _logger.Information("Streaming disabled");
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Erreur traitement {Method}", nameof(DisableStreaming));
            throw new ApiHandlerException();
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task PingHandlerAsync()
    {
        await PingAsync();
    }


    private Category GetCategory(string symbol)
    {
        switch (symbol)
        {
            case "FX":
                return Category.Forex;
            case "IND":
                return Category.Indices;
            default:
                return Category.Unknow;
        }
    }

    private string? ComputeCommentReasonClosed(string? comment)
    {
        switch (comment)
        {
            case "[S/L]":
                return ReasonClosed.Sl.ToString();
            case "[T/P]":
                return ReasonClosed.Tp.ToString();
            case not null when comment.Contains("S/O"):
                return ReasonClosed.Margin.ToString();
            default:
                return null;
        }
    }


    private TypePosition ConvertCmdToType(long? cmd)
    {
        switch (cmd)
        {
            case 0:
                return TypePosition.Buy;
            case 1:
                return TypePosition.Sell;
            case 2:
                return TypePosition.BuyLimit;
            case 3:
                return TypePosition.SellLimit;
            case 4:
                return TypePosition.BuyStop;
            case 5:
                return TypePosition.SellStop;
            case 6:
                return TypePosition.Balance;
            case 7:
                return TypePosition.Credit;
            default:
                throw new ArgumentException($"value {cmd} not handled for {nameof(TypePosition)}");
        }
    }

    private List<Position> MapTradeRecordsToPositions(List<TradeRecord> data, StatusPosition statusPosition)
    {
        var rsp = data
            .Select(x => new Position
            {
                TypePosition = ConvertCmdToType(x.Cmd),
                Profit = (decimal)x.Profit.GetValueOrDefault(),
                OpenPrice = (decimal)x.Open_price.GetValueOrDefault(),
                ClosePrice = (decimal)x.Close_price.GetValueOrDefault(),
                DateOpen = x.Open_time.GetValueOrDefault().ConvertToDatetime(),
                DateClose = x.Close_time.GetValueOrDefault().ConvertToDatetime(),
                ReasonClosed = ComputeCommentReasonClosed(x.Comment),
                StopLoss = (decimal?)x.Sl.GetValueOrDefault(),
                TakeProfit = (decimal?)x.Tp.GetValueOrDefault(),
                Volume = x.Volume.GetValueOrDefault(),
                StatusPosition = statusPosition,
                Comment = x.Comment,
                PositionId = x.Position,
                Order = x.Order,
                Order2 = x.Order2,
                Symbol = x.Symbol,
                CustomComment = x.CustomComment,
                StrategyId = x.CustomComment
            })
            .ToList();

        return rsp;
    }

    private List<Candle> ConvertChartResponseToCandlesList(LinkedList<RateInfoRecord> rateInfo,
        double tickSize)
    {
        return rateInfo
            .Select(x =>
                {
                    var open = (decimal)(x.Open.GetValueOrDefault() * tickSize);

                    return new Candle()
                        .SetOpen(open)
                        .SetHigh(open + (decimal)(x.High.GetValueOrDefault() * tickSize))
                        .SetLow(open + (decimal)(x.Low.GetValueOrDefault() * tickSize))
                        .SetClose(open + (decimal)(x.Close.GetValueOrDefault() * tickSize))
                        .SetDate(x.Ctm.GetValueOrDefault().ConvertToDatetime())
                        .SetVolume((decimal)x.Vol.GetValueOrDefault());
                }
            )
            .ToList();
    }


    private (PERIOD_CODE periodCode, DateTime dateTime) SetDateTime(Timeframe tf)
    {
        DateTime dateTime;
        PERIOD_CODE periodCodeData;
        switch (tf)
        {
            case Timeframe.OneMinute:
                dateTime = DateTime.Now.AddMonths(-1);
                periodCodeData = PERIOD_CODE.PERIOD_M1;
                return (periodCodeData, dateTime);

            case Timeframe.FiveMinutes:
                dateTime = DateTime.Now.AddMonths(-1);
                periodCodeData = PERIOD_CODE.PERIOD_M5;
                return (periodCodeData, dateTime);

            case Timeframe.FifteenMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_M15;
                return (periodCodeData, dateTime);
            case Timeframe.ThirtyMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_M30;
                return (periodCodeData, dateTime);
            case Timeframe.OneHour:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_H1;
                return (periodCodeData, dateTime);
            case Timeframe.FourHour:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_H4;
                return (periodCodeData, dateTime);
            case Timeframe.Daily:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_D1;
                return (periodCodeData, dateTime);
            case Timeframe.Weekly:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_W1;
                return (periodCodeData, dateTime);
            case Timeframe.Monthly:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_MN1;
                return (periodCodeData, dateTime);


            default:
                throw new ArgumentException("Periode code n'existe pas");
        }
    }

    private TRADE_OPERATION_CODE GetTradeOperationByTypePosition(TypePosition signal)
    {
        return signal switch
        {
            TypePosition.Buy => TRADE_OPERATION_CODE.BUY,
            TypePosition.Sell => TRADE_OPERATION_CODE.SELL,
            _ => throw new System.Exception("Le type n'existe pas")
        };
    }

    public static TRADE_OPERATION_CODE GetOperationCode(int code)
    {
        switch (code)
        {
            case 0:
                return TRADE_OPERATION_CODE.BUY;

            case 1:
                return TRADE_OPERATION_CODE.SELL;

            case 2:
                return TRADE_OPERATION_CODE.BUY_LIMIT;

            case 3:
                return TRADE_OPERATION_CODE.SELL_LIMIT;

            case 4:
                return TRADE_OPERATION_CODE.BUY_STOP;

            case 5:
                return TRADE_OPERATION_CODE.SELL_STOP;

            case 6:
                return TRADE_OPERATION_CODE.BALANCE;

            default:
                throw new ArgumentException($"Code {code} not handled for {nameof(TRADE_OPERATION_CODE)}");
        }
    }


    private void StreamingApiConnectorOnTickRecordReceived(StreamingTickRecord tickrecord)
    {
        if (tickrecord.Level == 0)
        {
            _logger.Verbose("Streaming Tick rececived {Tick}", tickrecord);
            var tick = new Tick((decimal?)tickrecord.Ask, (decimal?)tickrecord.Bid,
                tickrecord.Timestamp.GetValueOrDefault().ConvertToDatetime(), tickrecord.Symbol);
            tick.AskVolume = tickrecord.AskVolume;
            tick.BidVolume = tickrecord.BidVolume;
            TickEvent?.Invoke(this, tick);
        }
    }

    private void StreamingApiConnectorOnNewsRecordReceived(StreamingNewsRecord newsrecord)
    {
        var news = new News
        {
            Body = newsrecord.Body,
            Key = newsrecord.Key,
            Time = newsrecord.Time,
            Title = newsrecord.Title
        };

        NewsEvent?.Invoke(this, news);
    }

    private void StreamingApiConnectorOnBalanceRecordReceived(StreamingBalanceRecord balancerecord)
    {
        _logger.Verbose("Balance record received {@Balance}", balancerecord);

        AccountBalance.Balance = balancerecord.Balance.GetValueOrDefault();
        AccountBalance.Credit = balancerecord.Credit;
        AccountBalance.Equity = balancerecord.Equity;
        AccountBalance.Margin = balancerecord.Margin;
        AccountBalance.MarginFree = balancerecord.MarginFree;
        AccountBalance.MarginLevel = balancerecord.MarginLevel;

        NewBalanceEvent?.Invoke(this, AccountBalance);
    }

    private void StreamingApiConnectorOnProfitRecordReceived(StreamingProfitRecord profitrecord)
    {
        using (LogContext.PushProperty("Order", profitrecord.Order))
        using (LogContext.PushProperty("Order2", profitrecord.Order2))
        using (LogContext.PushProperty("PositionId", profitrecord.Position))
        {
            _logger.Verbose("Streaming Trade profit received {@Profitrecord}", profitrecord);


            var selected = _cachePosition.Find(x =>
                (x.Order != null && x.Order == profitrecord.Order) ||
                (x.Order2 != null && x.Order2 == profitrecord.Order2) ||
                (x.PositionId != null && x.PositionId == profitrecord.Position));

            if (selected is not null)
            {
                selected.Profit = (decimal)profitrecord.Profit.GetValueOrDefault();
                _logger.Verbose("Position profit updated  {@Id}", new { selected.Id, selected.Profit });
                PositionUpdatedEvent?.Invoke(this, selected);
            }
        }
    }

    private void StreamingApiConnectorOnTradeStatusRecordReceived(StreamingTradeStatusRecord tradestatusrecord)
    {
        using (LogContext.PushProperty("Order", tradestatusrecord.Order))
        {
            _logger.Information("Streaming Trade status record received {@Tradestatusrecord}", tradestatusrecord);
            if (tradestatusrecord.RequestStatus == REQUEST_STATUS.REJECTED)
            {
                var selected = _cachePosition
                    .Find(x =>
                        x.PositionId == tradestatusrecord.Order || x.Order == tradestatusrecord.Order ||
                        x.Order2 == tradestatusrecord.Order);

                if (selected is not null)
                {
                    selected.StatusPosition = StatusPosition.Rejected;
                    selected.Order = tradestatusrecord.Order;
                    selected.Comment = tradestatusrecord.CustomComment;
                    _logger.Information("Position {Id} rejected", selected.Id);
                    PositionRejectedEvent?.Invoke(this, selected);
                }
                else
                {
                    _logger.Warning("Position {Id} not in cache", tradestatusrecord.Order);
                }
            }
        }
    }

    private void StreamingApiConnectorOnTradeRecordReceived(StreamingTradeRecord traderecord)
    {
        try
        {
            using (LogContext.PushProperty("PositionId", traderecord.Position))
            using (LogContext.PushProperty("Order", traderecord.Order))
            using (LogContext.PushProperty("Order2", traderecord.Order2))
            {
                _logger.Information("Streaming trade record received {@Traderecord}", traderecord);
                var selected = _cachePosition
                    .Where(x => x.CustomComment != "")
                    .FirstOrDefault(x =>
                        x.PositionId == traderecord.Position || x.Order == traderecord.Order ||
                        x.Order2 == traderecord.Order2);
                if (selected is not null)
                    using (LogContext.PushProperty(PositionIdContext, selected.Id))
                    {
                        _logger.Information("Position found {@Selected}", selected);


                        var oldSl = selected.StopLoss;
                        var oldTp = selected.TakeProfit;

                        selected
                            .SetStopLoss((decimal?)traderecord.Sl)
                            .SetTakeProfit((decimal?)traderecord.Tp)
                            .SetDateOpen(traderecord.Open_time.GetValueOrDefault().ConvertToDatetime())
                            .SetTypePosition(ConvertCmdToType(traderecord.Cmd))
                            .SetOpenPrice((decimal)traderecord.Open_price.GetValueOrDefault())
                            .SetVolume(traderecord.Volume.GetValueOrDefault())
                            .SetComment(traderecord.Comment)
                            .SetProfit((decimal)traderecord.Profit.GetValueOrDefault());

                        if (traderecord.Order != traderecord.Order2)
                            selected
                                .SetOrder(traderecord.Order)
                                .SetOrder2(traderecord.Order2)
                                .SetPositionId(traderecord.Position);

                        if (traderecord.Type == STREAMING_TRADE_TYPE.PENDING)
                            _logger.Information("Trade in pending state {Id}", selected.Id);
                        else if (traderecord.Type == STREAMING_TRADE_TYPE.OPEN)
                            HandleOpenCallback(selected, oldSl, oldTp);
                        else if (traderecord.Type == STREAMING_TRADE_TYPE.CLOSE)
                            HandleCaseClosedPositionCallback(selected, traderecord);
                    }
                else
                    _logger.Warning("Position {@Id} not in system",
                        new { traderecord.Order, traderecord.Order2, traderecord.Position });
            }
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Error on streaming trade record received");
        }
    }

    private void HandleOpenCallback(Position selected, decimal? oldSl, decimal? oldTp)
    {
        if (selected.StatusPosition == StatusPosition.Pending &&
            selected.Order != selected.Order2)
        {
            _logger.Information("Trade opened {Id}", selected.Id);

            selected
                .SetStatusPosition(StatusPosition.Open);

            PositionOpenedEvent?.Invoke(this, selected);
        }
        else if (selected.StatusPosition == StatusPosition.Open)
        {
            var updateEvent =
                (selected.StopLoss != oldSl && oldSl != 0) ||
                (selected.TakeProfit != oldTp && oldTp != 0);

            if (updateEvent)
            {
                _logger.Information("Trade SL TP updated {Id}", selected.Id);
                PositionUpdatedEvent?.Invoke(this, selected);
            }
        }
    }

    private void HandleCaseClosedPositionCallback(Position selected, StreamingTradeRecord tradeRecord)
    {
        if (selected.StatusPosition is StatusPosition.Open or StatusPosition.WaitClose &&
            tradeRecord.Closed.GetValueOrDefault())
        {
            selected.SetStatusPosition(StatusPosition.Close)
                .SetDateClose(tradeRecord.Close_time.GetValueOrDefault().ConvertToDatetime())
                .SetClosePrice(new decimal(tradeRecord.Close_price.GetValueOrDefault()))
                .SetReasonClosed(ComputeCommentReasonClosed(selected.Comment))
                .SetProfit(new decimal(tradeRecord.Profit.GetValueOrDefault()));

            PositionClosedEvent?.Invoke(this, selected);
        }
    }
}