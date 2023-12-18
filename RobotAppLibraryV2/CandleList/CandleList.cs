using System.Diagnostics.CodeAnalysis;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;
using Serilog;
using Serilog.Context;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.CandleList;

public class CandleList : List<Candle>, ICandleList, IDisposable
{
    private const int MAX_CANDLE_COUNT = 2000;

    private readonly IApiHandler _apiHandler;
    private readonly ILogger _logger;

    private readonly string symbol;
    private readonly Timeframe timeframe;
    private TradeHourRecord _tradeHourRecord = new();


    public CandleList(IApiHandler apiHandler, ILogger logger, Timeframe timeframe, string symbol) : base(
        MAX_CANDLE_COUNT)
    {
        _apiHandler = apiHandler;
        this.timeframe = timeframe;
        this.symbol = symbol;
        _logger = logger.ForContext<CandleList>();
        Init();
    }

    private DateTime NextDateToRegister => this.Last().Date.AddMinutes(timeframe.GetMinuteFromTimeframe());

    private bool OnCorrecting { get; set; }

    private TradeHourRecord.HoursRecordData? CurrentHoursRecord =>
        _tradeHourRecord.HoursRecords.FirstOrDefault(x => x.Day == DateTime.UtcNow.DayOfWeek);

    private List<Tick> Ticks { get; } = new();
    public Tick? LastPrice => Ticks.LastOrDefault();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
            // Timeframe > 4Hours non gérer pour l'instant.
            if (timeframe > Timeframe.FourHour) throw new ArgumentException($"Timeframe {timeframe} non gérer");

            _apiHandler.TickEvent += ApiHandlerOnTickEvent;
            var data = _apiHandler.GetChartAsync(symbol, timeframe).Result;
            if (data is { Count: > 0 })
            {
                foreach (var candle in data.TakeLast(2000).ToList()) Add(candle);

                this.Validate();
            }

            _tradeHourRecord = _apiHandler.GetTradingHoursAsync(symbol).Result;
            HandlingStartTradeHours();
            _logger.Information("Candle list {Timeframe} initialized {@Candle}", timeframe, this.Last());
        }
        catch (Exception e)
        {
            throw new CandleListException("Can't initialize candle list", e);
        }
    }

    private async void ApiHandlerOnTickEvent(object? sender, Tick tick)
    {
        if (tick.Symbol == symbol)
        {
            var lastCandle = this.Last();

            Ticks.Add(tick);
            if (!OnCorrecting)
            {
                var mintoAdd = timeframe.GetMinuteFromTimeframe();
                var nextDate = lastCandle.Date.AddMinutes(mintoAdd);
                if (tick.Date < nextDate)
                {
                    UpdateLast(tick);
                    OnOnTickEvent(tick);
                    _logger.Verbose("New tick {@Tick}", tick);
                }
                else
                {
                    var nextDate2 = lastCandle.Date.AddMinutes(mintoAdd * 2);
                    if (tick.Date >= nextDate && tick.Date < nextDate2)
                    {
                        AddNewCandle(nextDate, tick);
                        OnOnCandleEvent(this.Last());
                        _logger.Information("New candle {@Candle}", this.Last());
                        if (NextDateToRegister.TimeOfDay >= CurrentHoursRecord.To)
                        {
                            _logger.Information("Next day to register depassed, need to be reset");

                            var nextValidHourRecord = GetNextValidHourRecord();
                            var dayDiff = (nextValidHourRecord.Day - DateTime.UtcNow.DayOfWeek + 7) % 7;

                            var isMidnightStart = nextValidHourRecord.From == TimeSpan.FromMilliseconds(0);
                            var isNextDay = dayDiff == 1;

                            if (!isNextDay || !isMidnightStart)
                                RegisterCandleForNewDate(GetNewNextDayDateRegisterFrom(nextValidHourRecord));
                            else
                                _logger.Information("No set necessary because of next start hour {TradeHour}",
                                    nextValidHourRecord);
                        }
                    }
                    else
                    {
                        await CorrectHistory(lastCandle.Date);
                    }
                }
            }
        }
    }

    private DateTime GetNewNextDayDateRegisterFrom(TradeHourRecord.HoursRecordData hoursRecordData)
    {
        _logger.Information("Get next day date with trading hour from");
        var dateRef = MatchDateFromHoursRecord(hoursRecordData);
        return GetNewDateFromOrTo(dateRef, hoursRecordData.From);
    }

    private DateTime MatchDateFromHoursRecord(TradeHourRecord.HoursRecordData hoursRecordData)
    {
        var dateTimeRef = DateTime.UtcNow.Date;

        while (dateTimeRef.DayOfWeek != hoursRecordData.Day) dateTimeRef = dateTimeRef.AddDays(+1);

        return dateTimeRef;
    }

    private void RegisterCandleForNewDate(DateTime date)
    {
        using (LogContext.PushProperty("Timeframe", timeframe))
        {
            _logger.Information("Register new candle for next date : {@NewDate}", date);
            var newDate = date;
            if (this.Last().Date != newDate)
            {
                var candle = new Candle()
                    .SetOpen(0)
                    .SetHigh(0)
                    .SetLow(0)
                    .SetClose(0)
                    .SetDate(newDate);
                _logger.Information("New candle to add {@CandleInfoNewObject}", candle);
                Add(candle);
                this.Validate();
            }
            else
            {
                _logger.Warning("Candle already existing {@LastCandle}", this.Last());
            }
        }
    }


    private void HandlingStartTradeHours()
    {
        var now = DateTime.UtcNow;
        _logger.Information("Adapting the start for timeframe {Timeframe} at {@Datetime}", timeframe, now);

        if (CurrentHoursRecord != null)
        {
            _logger.Information("Current trade hours is not null");

            var dateRefLimitDay = now.Date.AddDays(1).AddTicks(-1).TimeOfDay; // Optimisé pour minuit moins une seconde
            var isMidnightStart = CurrentHoursRecord.From == TimeSpan.Zero;
            var isEndTimeExceeded = CurrentHoursRecord.To >= dateRefLimitDay;

            if (isMidnightStart && isEndTimeExceeded)
            {
                _logger.Information("No set necessary because of next start hour {TradeHour}", CurrentHoursRecord);
            }
            else
            {
                var currentDate = now.Date;
                var dateToCheckFrom = currentDate.Add(CurrentHoursRecord.From);
                var dateToCheckTo = currentDate.Add(CurrentHoursRecord.To);

                if (NextDateToRegister < dateToCheckFrom)
                {
                    _logger.Information("Date 'from' inferior {@DateNow} | {@DateToCheck}", now, dateToCheckFrom);
                    RegisterCandleForNewDate(GetTodayDateRegisterFrom());
                }
                else if (NextDateToRegister >= dateToCheckTo)
                {
                    _logger.Information("Date 'to' depassed {@DateNow} | {@DateToCheck}", now, dateToCheckTo);
                    RegisterCandleForNewDate(GetNewNextDayDateRegisterFrom());
                }
            }
        }
        else
        {
            _logger.Information("The current hours record is null, updating to next hours record");
            RegisterCandleForNewDate(GetNewNextDayDateRegisterFrom());
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
        if (Count >= MAX_CANDLE_COUNT) RemoveAt(0);

        var price = tick.Bid.GetValueOrDefault();
        var candle = new Candle()
            .SetDate(dateTime)
            .SetOpen(price)
            .SetHigh(price)
            .SetLow(price)
            .SetClose(price);
        candle.Volume += tick.AskVolume.GetValueOrDefault();
        candle.Volume += tick.BidVolume.GetValueOrDefault();
        candle.AskVolume += (double)tick.AskVolume.GetValueOrDefault();
        candle.BidVolume += (double)tick.BidVolume.GetValueOrDefault();
        candle.Ticks.Add(tick);
        Add(candle);
    }

    private async Task CorrectHistory(DateTime start)
    {
        using (LogContext.PushProperty("Timeframe", timeframe))
        {
            _logger.Warning("Correct history, start = {Start} , last = {Last}", start,
                DateTime.UtcNow);

            var data = await _apiHandler.GetChartByDateAsync(symbol, timeframe, start, DateTime.UtcNow);

            if (data.Count > 0)
            {
                _logger.Information("Data fetched = {Start} , last = {Last}", data.First().Date,
                    data.Last().Date);

                while (data.First().Date == this.LastOrDefault()?.Date) Remove(this.Last());

                foreach (var candle in data.OrderBy(x => x.Date)) Add(candle);

                this.Validate();
            }
            else
            {
                _logger.Warning("No data fetched for correct history");
            }

            OnCorrecting = false;
        }
    }


    private void UpdateLast(Tick tick)
    {
        var last = this.Last();
        var lastTick = tick;
        last.Ticks.Add(lastTick);
        last.Close = lastTick.Bid.GetValueOrDefault();
        last.AskVolume += (double)lastTick.AskVolume.GetValueOrDefault();
        last.BidVolume += (double)lastTick.BidVolume.GetValueOrDefault();
        last.Volume += lastTick.AskVolume.GetValueOrDefault();
        last.Volume += lastTick.BidVolume.GetValueOrDefault();
        if (last.Open == 0) last.Open = tick.Bid.GetValueOrDefault();

        if (last.High == 0) last.High = tick.Bid.GetValueOrDefault();

        if (last.Low == 0) last.Low = tick.Bid.GetValueOrDefault();

        if (last.Close >= last.High)
            last.High = last.Close;
        else if (last.Close <= last.Low) last.Low = last.Close;
    }

    private DateTime GetNewNextDayDateRegisterFrom()
    {
        using (LogContext.PushProperty("Timeframe", timeframe))
        {
            _logger.Information("Get next day date with trading hour from");
            var localHourRecord = GetNextValidHourRecord();
            var dateRef = MatchDateFromHoursRecord(localHourRecord);
            return GetNewDateFromOrTo(dateRef, localHourRecord.From);
        }
    }

    private DateTime GetTodayDateRegisterFrom()
    {
        using (LogContext.PushProperty("Timeframe", timeframe))
        {
            _logger.Information("Get today date with trading hour from");

            return GetNewDateFromOrTo(DateTime.UtcNow.Date, CurrentHoursRecord.From);
        }
    }

    private TradeHourRecord.HoursRecordData GetNextValidHourRecord()
    {
        _logger.Information("Finding trade hours records for next day");

        TradeHourRecord.HoursRecordData? localHourRecord = null;
        var newDateDay = DateTime.UtcNow.Date.AddDays(1);

        while (localHourRecord is null)
        {
            localHourRecord = _tradeHourRecord.HoursRecords.FirstOrDefault(x => x.Day == newDateDay.DayOfWeek);
            if (localHourRecord is null) newDateDay = newDateDay.AddDays(1);
        }

        _logger.Information("The new Date day is {NewDateDay} for trade hours {@TradeHour}", newDateDay,
            localHourRecord);
        return localHourRecord;
    }


    private DateTime GetNewDateFromOrTo(DateTime dateTime, TimeSpan recordTime)
    {
        using (LogContext.PushProperty("Timeframe", timeframe))
        {
            var dateToValidate = dateTime;
            var dateToCompare = dateToValidate.Date.Add(recordTime);

            while (dateToValidate < dateToCompare)
                dateToValidate = dateToValidate.AddMinutes(timeframe.GetMinuteFromTimeframe());

            if (dateToValidate > dateToCompare)
                dateToValidate = dateToValidate.AddMinutes(-timeframe.GetMinuteFromTimeframe());

            _logger.Information("The new date to add is {DateToValidate}", dateToValidate);

            return dateToValidate;
        }
    }


    [ExcludeFromCodeCoverage]
    protected virtual void Dispose(bool disposing)
    {
    }
}