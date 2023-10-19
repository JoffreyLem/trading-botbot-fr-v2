using System.Globalization;
using RobotAppLibraryV2.ApiHandler.Exception;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.MoneyManagement;
using RobotAppLibraryV2.Utils;
using Serilog;
using Calendar = RobotAppLibraryV2.Modeles.Calendar;

namespace RobotAppLibraryV2.ApiHandler.Backtest;

public class BacktestApiHandler : IApiHandler
{
    private readonly IApiHandler _apiHandlerDelegate;
    private readonly List<Position> _cachePosition = new();
    private readonly LotValueCalculator _lotValueCalculator;
    private readonly SpreadSimulator _spreadSimulator;

    private Tick _lastPrice = new();

    public BacktestApiHandler(IApiHandler apiHandlerDelegate, SpreadSimulator spreadSimulator, ILogger logger,
        AccountBalance accountBalance, string symbol)
    {
        _apiHandlerDelegate = apiHandlerDelegate;
        _spreadSimulator = spreadSimulator;
        AccountBalance = accountBalance;
        _lotValueCalculator = new LotValueCalculator(apiHandlerDelegate, logger, symbol, false);
        Init(symbol);
    }

    public byte[] SymbolsCompressed { get; set; }
    public AccountBalance AccountBalance { get; set; }

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
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        return Task.CompletedTask;
    }

    public bool IsConnected()
    {
        return true;
    }

    public Task PingAsync()
    {
        return Task.CompletedTask;
    }

    public Task<AccountBalance> GetBalanceAsync()
    {
        return Task.FromResult(AccountBalance);
    }

    public Task<List<Position>> GetAllPositionsAsync()
    {
        return Task.FromResult(new List<Position>());
    }

    public Task<List<Calendar>> GetCalendarAsync()
    {
        return Task.FromResult(new List<Calendar>());
    }

    public Task<List<string>?> GetAllSymbolsAsync()
    {
        return Task.FromResult(new List<string>());
    }

    public Task<List<Position>> GetCurrentTradesAsync()
    {
        var selected = _cachePosition.Where(x => x.StatusPosition != StatusPosition.Close).ToList();
        return Task.FromResult(selected);
    }

    public Task<List<Position>> GetAllPositionsByCommentAsync(string comment)
    {
        return Task.FromResult(new List<Position>());
    }

    public async Task<SymbolInfo> GetSymbolInformationAsync(string symbol)
    {
        return await _apiHandlerDelegate.GetSymbolInformationAsync(symbol);
    }

    public async Task<TradeHourRecord> GetTradingHoursAsync(string symbol)
    {
        return await _apiHandlerDelegate.GetTradingHoursAsync(symbol);
    }

    public async Task<List<Candle>> GetChartAsync(string symbol, Timeframe timeframe)
    {
        return await _apiHandlerDelegate.GetChartAsync(symbol, timeframe);
    }

    public async Task<List<Candle>> GetChartByDateAsync(string symbol, Timeframe periodCodeStr, DateTime start,
        DateTime end)
    {
        return await _apiHandlerDelegate.GetChartByDateAsync(symbol, periodCodeStr, start, end);
    }

    public async Task<Tick> GetTickPriceAsync(string symbol)
    {
        return await _apiHandlerDelegate.GetTickPriceAsync(symbol);
    }

    public Task<Position> OpenPositionAsync(Position position)
    {
        position.StatusPosition = StatusPosition.Open;
        position.DateOpen = _lastPrice.Date;
        position.OpenPrice = _lastPrice.Bid.GetValueOrDefault();
        _cachePosition.Add(position);
        PositionOpenedEvent?.Invoke(this, position);
        return Task.FromResult(new Position());
    }

    public Task UpdatePositionAsync(decimal price, Position position)
    {
        var selected = _cachePosition.FirstOrDefault(x => x.Id == position.Id);

        if (selected is null) throw new ApiHandlerException($"Position {position.Id} does not exist");

        selected.StopLoss = position.StopLoss;
        selected.TakeProfit = position.TakeProfit;
        PositionUpdatedEvent?.Invoke(this, selected);
        return Task.CompletedTask;
    }

    public Task ClosePositionAsync(decimal price, Position position)
    {
        var selected = _cachePosition.FirstOrDefault(x => x.Id == position.Id);

        if (selected is null) throw new ApiHandlerException($"Position {position.Id} does not exist");

        selected.StatusPosition = StatusPosition.Close;
        selected.ClosePrice = _lastPrice.Bid.GetValueOrDefault();
        selected.DateClose = _lastPrice.Date;

        CalculateProfitPosition(selected);
        PositionClosedEvent?.Invoke(this, selected);
        return Task.CompletedTask;
    }

    public Task<bool> CheckIfSymbolExistAsync(string symbol)
    {
        return Task.FromResult(true);
    }

    public void SubscribePrice(string symbol)
    {
    }


    public void UnsubscribePrice(string symbol)
    {
    }

    private void Init(string symbol)
    {
        _lastPrice = _apiHandlerDelegate.GetTickPriceAsync(symbol).Result;
    }

    public async Task StartAsync(string symbol, Timeframe timeframe)
    {
        var chart = await GetChartAsync(symbol, timeframe);
        await Task.Run(() =>
        {
            foreach (var candle in chart)
            {
                var listTick = CutCandleToListTick(candle, timeframe);

                foreach (var tick in listTick.OrderBy(x => x.Date))
                {
                    _lastPrice = tick;
                    TickEvent?.Invoke(this, tick);
                    HandlePositions();
                }
            }
        });
    }

    private void HandlePositions()
    {
        var selectedPosition = _cachePosition.Where(x => x.StatusPosition != StatusPosition.Close).ToList();
        if (selectedPosition.Count > 0)
            foreach (var position in selectedPosition)
            {
                var closePrice = _lastPrice.Bid;

                if (position.TypePosition == TypePosition.Buy)
                {
                    position.DateClose = _lastPrice.Date;
                    if (closePrice <= position.StopLoss)
                    {
                        position.ClosePrice = (decimal)position.StopLoss;
                        position.StatusPosition = StatusPosition.Close;
                    }
                    else if (closePrice >= position.TakeProfit)
                    {
                        position.ClosePrice = (decimal)position.TakeProfit;
                        position.StatusPosition = StatusPosition.Close;
                    }
                }
                else if (position.TypePosition == TypePosition.Sell)
                {
                    position.DateClose = _lastPrice.Date;
                    if (closePrice >= position.StopLoss)
                    {
                        position.ClosePrice = (decimal)position.StopLoss;
                        position.StatusPosition = StatusPosition.Close;
                    }
                    else if (closePrice <= position.TakeProfit)
                    {
                        position.ClosePrice = (decimal)position.TakeProfit;
                        position.StatusPosition = StatusPosition.Close;
                    }
                }

                CalculateProfitPosition(position);
                if (position.StatusPosition == StatusPosition.Close)
                    PositionClosedEvent?.Invoke(this, position);
                else
                    PositionUpdatedEvent?.Invoke(this, position);
            }
    }

    private void CalculateProfitPosition(Position position)
    {
        var profit = 0.0m;
        var closePrice = position.ClosePrice;
        var openPrice = position.OpenPrice;
        var spread = (decimal)position.Spread;
        var precision = GetPricePrecision(openPrice);
        var adjustedSpread = spread / (decimal)Math.Pow(10, precision);

        var volume = (decimal)position.Volume;
        var lotValue = (decimal)_lotValueCalculator.LotValueStandard;

        switch (position.TypePosition)
        {
            case TypePosition.Buy:
                profit = (closePrice - openPrice - adjustedSpread) * volume * lotValue;
                break;

            case TypePosition.Sell:
                profit = (openPrice - closePrice - adjustedSpread) * volume * lotValue;
                break;

            default:
                throw new ArgumentException("Type de position non reconnu.");
        }

        position.Profit = profit;
    }


    private List<Tick> CutCandleToListTick(Candle candle, Timeframe timeframe)
    {
        var timeDivisor = (double)timeframe.GetMinuteFromTimeframe() / 4;

        var ticks = new List<Tick>();

        var startTime = candle.Date.AddMinutes(timeframe.GetMinuteFromTimeframe());

        var bid1 = candle.Open;
        var time1 = startTime;
        var tick1 = new Tick()
            .SetBid(bid1)
            .SetAsk(bid1 + _spreadSimulator.GenerateSpread())
            .SetDate(time1);
        ticks.Add(tick1);

        var bid2 = candle.High;
        var time2 = startTime.AddMinutes(timeDivisor);
        var tick2 = new Tick()
            .SetBid(bid2)
            .SetAsk(bid2 + _spreadSimulator.GenerateSpread())
            .SetDate(time2);
        ticks.Add(tick2);

        var bid3 = candle.Low;
        var time3 = time2.AddMinutes(timeDivisor);
        var tick3 = new Tick()
            .SetBid(bid3)
            .SetAsk(bid3 + _spreadSimulator.GenerateSpread())
            .SetDate(time3);
        ticks.Add(tick3);

        var bid4 = candle.Close;
        var time4 = time3.AddMinutes(timeDivisor).AddSeconds(-1);
        var tick4 = new Tick()
            .SetBid(bid4)
            .SetAsk(bid4 + _spreadSimulator.GenerateSpread())
            .SetDate(time4);
        ticks.Add(tick4);

        return ticks;
    }

    // TODO : Généraliser pour toutes les précisions ? 
    private int GetPricePrecision(decimal price)
    {
        var priceStr = price.ToString(CultureInfo.InvariantCulture);
        var decimalIndex = priceStr.IndexOf('.');
        return decimalIndex == -1 ? 0 : priceStr.Length - decimalIndex - 1;
    }

    public void Dispose()
    {
    }
}