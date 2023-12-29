using RobotAppLibraryV2.ApiConnector.Interfaces;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.MoneyManagement;
using Serilog;

namespace RobotAppLibraryV2.BackTest;

public class BackTestApiExecutor : ICommandExecutor
{
    private readonly IApiHandler _apiHandlerProxy;
    private readonly ILogger _logger;

    private readonly List<Position> _positionsCache = new();

    private readonly LotValueCalculator LotValueCalculator;

    private readonly SymbolInfo SymbolInfo;

    private List<Candle> _candles = new();

    private Tick _currentTick = new();


    public BackTestApiExecutor(IApiHandler apiHandlerProxy, ILogger logger, BacktestParameters backtestParameters)
    {
        _apiHandlerProxy = apiHandlerProxy;
        _logger = logger;
        BacktestParameters = backtestParameters;
        AccountBalance = new AccountBalance
        {
            Balance = backtestParameters.Balance,
            Credit = backtestParameters.Balance,
            MarginLevel = backtestParameters.Balance,
            MarginFree = backtestParameters.Balance,
            Margin = backtestParameters.Balance,
            Equity = backtestParameters.Balance
        };
        LotValueCalculator = new LotValueCalculator(apiHandlerProxy, logger, backtestParameters.Symbol);
        SymbolInfo = _apiHandlerProxy.GetSymbolInformationAsync(backtestParameters.Symbol).GetAwaiter()
            .GetResult();
    }

    public AccountBalance AccountBalance { get; set; }

    public BacktestParameters BacktestParameters { get; set; }

    public event Action<Tick>? TickRecordReceived;
    public event Action<Position?>? TradeRecordReceived;
    public event Action<AccountBalance?>? BalanceRecordReceived;
    public event Action<Position>? ProfitRecordReceived;
    public event Action<News>? NewsRecordReceived;
    public event Action? KeepAliveRecordReceived;
    public event Action<Candle>? CandleRecordReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public Task ExecuteLoginCommand(Credentials credentials)
    {
        return Task.CompletedTask;
    }

    public Task ExecuteLogoutCommand()
    {
        return Task.CompletedTask;
    }

    public Task<List<SymbolInfo>> ExecuteAllSymbolsCommand()
    {
        return _apiHandlerProxy.GetAllSymbolsAsync();
    }

    public Task<List<CalendarData>> ExecuteCalendarCommand()
    {
        return Task.FromResult(new List<CalendarData>());
    }

    public Task<List<Candle>> ExecuteFullChartCommand(Timeframe timeframe, DateTime start, string symbol)
    {
        var candles = _apiHandlerProxy.GetChartAsync(BacktestParameters.Symbol, BacktestParameters.Timeframe).Result;
        var dataToReturn = candles.GetRange(0, 2000);
        candles.RemoveRange(0, 2000);
        _candles = candles;
        return Task.FromResult(dataToReturn);
    }

    public Task<List<Candle>> ExecuteRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end, string symbol)
    {
        return Task.FromResult(new List<Candle>());
    }

    public Task<AccountBalance?> ExecuteBalanceAccountCommand()
    {
        return Task.FromResult(AccountBalance);
    }

    public Task<List<News>> ExecuteNewsCommand(DateTime? start, DateTime? end)
    {
        return Task.FromResult(new List<News>());
    }

    public Task<string> ExecuteCurrentUserDataCommand()
    {
        return Task.FromResult("");
    }

    public Task<bool> ExecutePingCommand()
    {
        return Task.FromResult(true);
    }

    public Task<SymbolInfo> ExecuteSymbolCommand(string symbol)
    {
        return _apiHandlerProxy.GetSymbolInformationAsync(symbol);
    }

    public Task<Tick> ExecuteTickCommand(string symbol)
    {
        return _apiHandlerProxy.GetTickPriceAsync(symbol);
    }

    public Task<List<Position>> ExecuteTradesHistoryCommand(string positionReference)
    {
        var trades = _positionsCache.Where(x => x.StatusPosition == StatusPosition.Close).ToList();
        return Task.FromResult(trades);
    }

    public Task<Position?> ExecuteTradesOpenedTradesCommand(string positionReference)
    {
        return Task.FromResult<Position?>(null);
    }

    public Task<TradeHourRecord> ExecuteTradingHoursCommand(string symbol)
    {
        var tradeHourRecord = new TradeHourRecord();
        var dateRefLimitDay = DateTime.UtcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59).TimeOfDay;
        tradeHourRecord.HoursRecords.Add(new TradeHourRecord.HoursRecordData
        {
            Day = DateTime.UtcNow.Date.DayOfWeek,
            From = TimeSpan.Zero,
            To = dateRefLimitDay
        });

        return Task.FromResult(tradeHourRecord);
    }

    public Task<Position> ExecuteOpenTradeCommand(Position position, decimal price)
    {
        var newPos = position.Clone();
        newPos.DateOpen = _currentTick.Date;
        newPos.StatusPosition = StatusPosition.Open;
        newPos.Order = Guid.NewGuid().ToString();
        newPos.OpenPrice = _currentTick.Bid.GetValueOrDefault();
        _positionsCache.Add(newPos);
        HandleProfitPositions(newPos);
        return Task.FromResult(newPos);
    }

    public Task<Position> ExecuteUpdateTradeCommand(Position position, decimal price)
    {
        var positionCache = _positionsCache.FirstOrDefault(x => x.Order == position.Order);
        if (positionCache is null)
        {
            _logger.Fatal("Position order {Position} not found for update", position.Order);
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        positionCache.StopLoss = position.StopLoss;
        positionCache.TakeProfit = position.TakeProfit;
        HandleProfitPositions(positionCache);
        return Task.FromResult(positionCache);
    }

    public Task<Position> ExecuteCloseTradeCommand(Position position, decimal price)
    {
        var positionCache = _positionsCache.FirstOrDefault(x => x.Order == position.Order);
        if (positionCache is null)
        {
            _logger.Fatal("Position order {Position} not found for close", position.Order);
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        positionCache.DateClose = _currentTick.Date;
        positionCache.ClosePrice = _currentTick.Bid;
        positionCache.StatusPosition = StatusPosition.Close;
        positionCache.ReasonClosed = ReasonClosed.Closed;
        HandleProfitPositions(positionCache);
        return Task.FromResult(positionCache);
    }

    public bool ExecuteIsConnected()
    {
        return true;
    }

    public void ExecuteSubscribeBalanceCommandStreaming()
    {
    }

    public void ExecuteStopBalanceCommandStreaming()
    {
    }

    public void ExecuteSubscribeCandleCommandStreaming(string symbol)
    {
    }

    public void ExecuteStopCandleCommandStreaming(string symbol)
    {
    }

    public void ExecuteSubscribeKeepAliveCommandStreaming()
    {
    }

    public void ExecuteStopKeepAliveCommandStreaming()
    {
    }

    public void ExecuteSubscribeNewsCommandStreaming()
    {
    }

    public void ExecuteStopNewsCommandStreaming()
    {
    }

    public void ExecuteSubscribeProfitsCommandStreaming()
    {
    }

    public void ExecuteStopProfitsCommandStreaming()
    {
    }

    public void ExecuteTickPricesCommandStreaming(string symbol)
    {
    }

    public void ExecuteStopTickPriceCommandStreaming(string symbol)
    {
    }

    public void ExecuteTradesCommandStreaming()
    {
    }

    public void ExecuteStopTradesCommandStreaming()
    {
    }

    public void ExecuteTradeStatusCommandStreaming()
    {
    }

    public void ExecuteStopTradeStatusCommandStreaming()
    {
    }

    public void ExecutePingCommandStreaming()
    {
    }

    public void ExecuteStopPingCommandStreaming()
    {
    }

    public void Dispose()
    {
        LotValueCalculator?.Dispose();
        _positionsCache.Clear();
        GC.SuppressFinalize(this);
    }

    public async Task StartBackTest()
    {
        var symbolInfo = await _apiHandlerProxy.GetSymbolInformationAsync(BacktestParameters.Symbol);

        await Task.Run(() =>
        {
            var allTicks = _candles.SelectMany(candle =>
                CandleHelper.DecomposeCandlestick(candle, BacktestParameters.Timeframe,
                    BacktestParameters.SpreadSimulator.GenerateSpread(), symbolInfo)
            ).ToList();

            foreach (var tick in allTicks)
            {
                _currentTick = tick;
                TickRecordReceived?.Invoke(tick);
                HandlePositionOpened();
            }

            Disconnected?.Invoke(this, EventArgs.Empty);
        });
    }

    private void HandlePositionOpened()
    {
        var positionOpened = _positionsCache.Where(x => x.StatusPosition != StatusPosition.Close).ToList();
        if (positionOpened is not { Count: > 0 }) return;
        foreach (var position in positionOpened)
            if (position.TypePosition == TypeOperation.Buy)
                HandlePositionBuy(position);
            else
                HandlePositionSell(position);
    }

    private void HandleProfitPositions(Position position)
    {
        var openPrice = position.OpenPrice;
        var closePrice = position.ClosePrice ?? _currentTick.Bid.GetValueOrDefault();

        var pips = position.TypePosition == TypeOperation.Buy
            ? closePrice - openPrice
            : openPrice - closePrice;
        var pipsSpread = _currentTick.Spread.GetValueOrDefault();

        if (SymbolInfo.Category == Category.Forex)
        {
            var tickPrecision = SymbolInfo.Symbol.Contains("JPY") ? 2 : 4;
            pips *= (decimal)Math.Pow(10, tickPrecision);
            pipsSpread *= (decimal)Math.Pow(10, tickPrecision);
        }

        var spreadValue = pipsSpread * (decimal)LotValueCalculator.PipValueStandard;

        var profitValue = (decimal)LotValueCalculator.PipValueStandard * pips - spreadValue;
        position.Profit = profitValue;
    }

    private void HandleCallBackPosition(Position position)
    {
        if (position.ClosePrice is not null)
        {
            position.StatusPosition = StatusPosition.Close;
            position.DateClose = _currentTick.Date;
            TradeRecordReceived?.Invoke(position);
        }
        else
        {
            position.StatusPosition = StatusPosition.Updated;
            TradeRecordReceived?.Invoke(position);
        }
    }

    private void HandlePositionBuy(Position position)
    {
        var currentPrice = _currentTick.Bid.GetValueOrDefault();

        if (currentPrice <= position.StopLoss)
        {
            position.ClosePrice = position.StopLoss;
            position.ReasonClosed = ReasonClosed.Sl;
        }
        else if (currentPrice >= position.TakeProfit)
        {
            position.ClosePrice = position.TakeProfit;
            position.ReasonClosed = ReasonClosed.Tp;
        }

        HandleProfitPositions(position);
        HandleCallBackPosition(position);
    }

    private void HandlePositionSell(Position position)
    {
        var currentPrice = _currentTick.Bid.GetValueOrDefault();

        if (currentPrice >= position.StopLoss)
        {
            position.ClosePrice = position.StopLoss;
            position.ReasonClosed = ReasonClosed.Sl;
        }
        else if (currentPrice <= position.TakeProfit)
        {
            position.ClosePrice = position.TakeProfit;
            position.ReasonClosed = ReasonClosed.Tp;
        }

        HandleProfitPositions(position);
        HandleCallBackPosition(position);
    }
}