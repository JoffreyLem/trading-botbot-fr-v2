using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;
using Serilog;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.CandleList;

public class CandleList : List<Candle>, ICandleList
{
    private readonly IApiHandler _apiHandler;
    private readonly ILogger _logger;

    private readonly string symbol;
    private readonly Timeframe timeframe;
    private TradeHourRecord _tradeHourRecord = new();


    public CandleList(IApiHandler apiHandler, ILogger logger, Timeframe timeframe, string symbol) : base(2100)
    {
        _apiHandler = apiHandler;
        this.timeframe = timeframe;
        this.symbol = symbol;
        _logger = logger.ForContext<CandleList>();
        Init();
    }

    private TradeHourRecord.HoursRecordData? CurrentHoursRecord =>
        _tradeHourRecord.HoursRecords.FirstOrDefault(x => x.Day == DateTime.UtcNow.DayOfWeek);

    public Tick? LastPrice { get; private set; }


    public event Func<Tick, Task>? OnTickEvent;
    public event Func<Candle, Task>? OnCandleEvent;

    public IEnumerable<Candle> Aggregate(Timeframe timeframeData)
    {
        return this.Aggregate(timeframeData.ToPeriodSize()).Select(x => new Candle()
            .SetOpen(x.Open)
            .SetHigh(x.High)
            .SetLow(x.Low)
            .SetClose(x.Close)
            .SetDate(x.Date));
    }


    private void Init()
    {
        try
        {
            _apiHandler.TickEvent += ApiHandlerOnTickEvent;
            var data = _apiHandler.GetChartAsync(symbol, timeframe).Result;
            if (data is { Count: > 0 })
            {
                foreach (var candle in data.TakeLast(2000).ToList()) Add(candle);

                this.Validate();
            }

            _tradeHourRecord = _apiHandler.GetTradingHoursAsync(symbol).Result;

            _logger.Information("Candle list {Timeframe} initialized {@Candle}", timeframe, this.LastOrDefault());
        }
        catch (Exception e)
        {
            throw new CandleListException("Can't initialize candle list", e);
        }
    }

    private void ApiHandlerOnTickEvent(object? sender, Tick tick)
    {
        if (tick.Symbol == symbol)
        {
            LastPrice = tick;
            var candleStartTimeTick = CalculateCandleStartTime(tick.Date);
            var minutes = timeframe.GetMinuteFromTimeframe();

            var lastDateToVerify = candleStartTimeTick.Date.AddMinutes(minutes);

            if (Count == 0 || this.Last().Date != candleStartTimeTick)
            {
                AddNewCandle(candleStartTimeTick, tick);
                OnOnCandleEvent(this.Last());

                // TODO : Reimplementer le correct history en prenant compte les trades hours sur cette méthode
            }
            else
            {
                UpdateLast(tick);
                OnOnTickEvent(tick);
            }
        }
    }

    protected virtual void OnOnTickEvent(Tick obj)
    {
        OnTickEvent?.Invoke(obj);
    }

    protected virtual void OnOnCandleEvent(Candle obj)
    {
        OnCandleEvent?.Invoke(obj);
    }

    private void AddNewCandle(DateTime dateTime, Tick tick)
    {
        if (Count >= 2000) RemoveAt(0);

        var price = tick.Bid.GetValueOrDefault();
        var candle = new Candle()
            .SetDate(dateTime)
            .SetOpen(price)
            .SetHigh(price)
            .SetLow(price)
            .SetClose(price);
        candle.Volume += tick.AskVolume.GetValueOrDefault();
        candle.Volume += tick.BidVolume.GetValueOrDefault();
        candle.AskVolume += tick.AskVolume.GetValueOrDefault();
        candle.BidVolume += tick.BidVolume.GetValueOrDefault();
        candle.Ticks.Add(tick);
        Add(candle);
    }

    private void UpdateLast(Tick tick)
    {
        var last = this.Last();
        var lastTick = tick;
        last.Ticks.Add(lastTick);
        last.Close = lastTick.Bid.GetValueOrDefault();
        last.AskVolume += lastTick.AskVolume.GetValueOrDefault();
        last.BidVolume += lastTick.BidVolume.GetValueOrDefault();
        last.Volume += lastTick.AskVolume.GetValueOrDefault();
        last.Volume += lastTick.BidVolume.GetValueOrDefault();
        if (last.Open == 0) last.Open = tick.Bid.GetValueOrDefault();

        if (last.High == 0) last.High = tick.Bid.GetValueOrDefault();

        if (last.Low == 0) last.Low = tick.Bid.GetValueOrDefault();

        if (last.Close >= last.High)
            last.High = last.Close;
        else if (last.Close <= last.Low) last.Low = last.Close;
    }

    private DateTime CalculateCandleStartTime(DateTime tickTime)
    {
        var totalMinutesTimeframe = timeframe.GetMinuteFromTimeframe();

        if (timeframe == Timeframe.Monthly) return new DateTime(tickTime.Year, tickTime.Month, 1, 0, 0, 0);

        if (timeframe == Timeframe.Weekly)
        {
            var dayOfWeek = (int)tickTime.DayOfWeek;
            var daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            var startOfWeek = tickTime.Date.AddDays(-daysToSubtract);
            return new DateTime(startOfWeek.Year, startOfWeek.Month, startOfWeek.Day, 0, 0, 0);
        }

        if (timeframe == Timeframe.Daily)
            return new DateTime(tickTime.Year, tickTime.Month, tickTime.Day, 0, 0, 0, DateTimeKind.Utc);

        var tickTimeTotalMinutes = tickTime.Hour * 60 + tickTime.Minute;

        var candleStartTotalMinutes = tickTimeTotalMinutes / totalMinutesTimeframe * totalMinutesTimeframe;

        var candleStartHour = candleStartTotalMinutes / 60;
        var candleStartMinute = candleStartTotalMinutes % 60;

        return new DateTime(tickTime.Year, tickTime.Month, tickTime.Day, candleStartHour, candleStartMinute, 0);
    }
}