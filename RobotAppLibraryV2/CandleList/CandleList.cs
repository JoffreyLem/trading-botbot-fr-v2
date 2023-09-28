using System.Diagnostics.CodeAnalysis;
using System.Timers;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;
using Serilog;
using Serilog.Context;
using Skender.Stock.Indicators;
using Timer = System.Timers.Timer;

namespace RobotAppLibraryV2.CandleList;

public class CandleList : List<Candle>, IDisposable
{
    private readonly IApiHandler _apiHandler;
    private readonly ILogger _logger;

    public readonly string Symbol;
    public readonly Timeframe Timeframe;
    private Timer? _timer;
    private TradeHourRecord _tradeHourRecord = new();


    public CandleList(IApiHandler apiHandler, ILogger logger, Timeframe timeframe, string symbol) : base(2100)
    {
        _apiHandler = apiHandler;
        Timeframe = timeframe;
        Symbol = symbol;
        _logger = logger.ForContext<CandleList>();
        Init();
    }

    private DateTime NextDateToRegister => this.Last().Date.AddMinutes(Timeframe.GetMinuteFromTimeframe());

    public bool OnCorrecting { get; private set; }
    public decimal? Spread => LastPrice.GetValueOrDefault().Spread;
    public Tick? LastPrice => Ticks.LastOrDefault();

    public TradeHourRecord.HoursRecordData? CurrentHoursRecord =>
        _tradeHourRecord.HoursRecords.FirstOrDefault(x => x.Day == DateTime.Now.DayOfWeek);

    public List<Tick> Ticks { get; } = new();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event Action<Tick>? OnTickEvent;
    public event Action<Candle>? OnCandleEvent;

    private void Init()
    {
        try
        {
            // Timeframe > Daily non gérer pour l'instant.
            if (Timeframe > (Timeframe)6) throw new ArgumentException($"Timeframe {Timeframe} non gérer");

            _apiHandler.TickEvent += ApiHandlerOnTickEvent;
            var data = _apiHandler.GetChartAsync(Symbol, Timeframe).Result;
            if (data is { Count: > 0 })
            {
                foreach (var candle in data.TakeLast(2000).ToList()) Add(candle);

                this.Validate();
            }

            _tradeHourRecord = _apiHandler.GetTradingHoursAsync(Symbol).Result;
            HandlingStartTradeHours();

            // TODO : Voir comment faire des TU sur le timer.
            SetTimerJobTradingHour();
            _logger.Information("Candle list {Timeframe} initialized {@Candle}", Timeframe, this.Last());
        }
        catch (Exception e)
        {
            throw new CandleListException("Can't initialize candle list", e);
        }
    }

    private async void ApiHandlerOnTickEvent(object? sender, Tick tick)
    {
        if (tick.Symbol == Symbol)
        {
            var lastCandle = this.Last();

            Ticks.Add(tick);
            if (!OnCorrecting)
            {
                var mintoAdd = Timeframe.GetMinuteFromTimeframe();
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
                    }
                    else
                    {
                        await CorrectHistory(lastCandle.Date);
                    }
                }
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
        candle.AskVolume += (double)tick.AskVolume.GetValueOrDefault();
        candle.BidVolume += (double)tick.BidVolume.GetValueOrDefault();
        candle.Ticks.Add(tick);
        Add(candle);
    }

    private async Task CorrectHistory(DateTime start)
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _logger.Warning("Correct history, start = {Start} , last = {Last}", start,
                DateTime.Now);

            var data = await _apiHandler.GetChartByDateAsync(Symbol, Timeframe, start, DateTime.Now);

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

    private void HandlingStartTradeHours()
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _logger.Information("Adapting the start for timeframe {Timeframe} at {@Datetime}", Timeframe, DateTime.Now);

            if (CurrentHoursRecord is not null)
            {
                _logger.Information("Current trade hours is not null");

                var dateRefLimitDay = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).TimeOfDay;

                if (CurrentHoursRecord?.From == TimeSpan.FromMilliseconds(0) &&
                    CurrentHoursRecord?.To >= dateRefLimitDay)
                {
                    _logger.Information("No set necessary");
                    return;
                }

                var dateToTcheckFrom = DateTime.Now.Date.Add(CurrentHoursRecord!.From);
                var dateToTcheckTo = DateTime.Now.Date.Add(CurrentHoursRecord!.To);

                if (NextDateToRegister < dateToTcheckFrom)
                {
                    _logger.Information("Date 'from' inferior {@DateNow} | {@DateToCheck}", DateTime.Now,
                        dateToTcheckFrom);
                    RegisterCandleForNewDate(GetTodayDateRegisterFrom());
                }
                else if (NextDateToRegister >= dateToTcheckTo)
                {
                    _logger.Information("Date 'to' depassed {@DateNow} | {@DateToCheck}", DateTime.Now, dateToTcheckTo);
                    RegisterCandleForNewDate(GetNewNextDayDateRegisterFrom());
                }
            }
            else
            {
                _logger.Information("The current hours record is null, updating to next hours record");
                RegisterCandleForNewDate(GetNewNextDayDateRegisterFrom());
            }
        }
    }

    private DateTime GetNewNextDayDateRegisterFrom()
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _logger.Information("Get next day date with trading hour from");
            var localHourRecord = GetNextValidHourRecord();
            var dateRef = MatchDateFromHoursRecord(localHourRecord);
            return GetNewDateFromOrTo(dateRef, localHourRecord.From);
        }
    }

    private DateTime GetNewNextDayDateRegisterTo()
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _logger.Information("Get next day date with trading hour to");
            var localHourRecord = GetNextValidHourRecord();
            var dateRef = MatchDateFromHoursRecord(localHourRecord);
            return GetNewDateFromOrTo(dateRef, localHourRecord.To);
        }
    }

    private DateTime GetTodayDateRegisterFrom()
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _logger.Information("Get today date with trading hour from");
            var localHourRecord = CurrentHoursRecord;
            return GetNewDateFromOrTo(DateTime.Now.Date, localHourRecord.From);
        }
    }

    private DateTime GetTodayDateRegisterTo()
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _logger.Information("Get today date with trading hour to");
            var localHourRecord = CurrentHoursRecord;
            return GetNewDateFromOrTo(DateTime.Now.Date, localHourRecord.To);
        }
    }

    private DateTime MatchDateFromHoursRecord(TradeHourRecord.HoursRecordData hoursRecordData)
    {
        var dateTimeRef = DateTime.Now.Date;

        while (dateTimeRef.DayOfWeek != hoursRecordData.Day) dateTimeRef = dateTimeRef.AddDays(+1);

        return dateTimeRef;
    }

    private TradeHourRecord.HoursRecordData GetNextValidHourRecord()
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            TradeHourRecord.HoursRecordData? localHourRecord = null;
            var newDateDay = DateTime.Now.Date.AddDays(1);
            while (localHourRecord is null)
            {
                localHourRecord = _tradeHourRecord.HoursRecords.FirstOrDefault(x => x.Day == newDateDay.DayOfWeek);
                if (localHourRecord is null) newDateDay = newDateDay.AddDays(1);
            }

            _logger.Information("The new Date day is {NewDateDay} for trade hours {@TradeHour}", newDateDay,
                localHourRecord);
            return localHourRecord;
        }
    }


    private DateTime GetNewDateFromOrTo(DateTime dateTime, TimeSpan recordTime)
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            var dateToValidate = dateTime;
            var dateToCompare = dateToValidate.Date.Add(recordTime);

            while (dateToValidate < dateToCompare)
                dateToValidate = dateToValidate.AddMinutes(Timeframe.GetMinuteFromTimeframe());

            if (dateToValidate > dateToCompare)
                dateToValidate = dateToValidate.AddMinutes(-Timeframe.GetMinuteFromTimeframe());

            _logger.Information("The new date to add is {DateToValidate}", dateToValidate);

            return dateToValidate;
        }
    }

    private void SetTimerJobTradingHour()
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _logger.Information("Setting the timer for next reschedule");
            DateTime runAt;

            if (CurrentHoursRecord is not null && NextDateToRegister < DateTime.Now.Date.Add(CurrentHoursRecord.To))
                runAt = GetTodayDateRegisterTo();
            else
                runAt =
                    GetNewNextDayDateRegisterTo();

            _logger.Information("Next timer run defined at: {RunAt}", runAt);
            var dueTime = runAt - DateTime.Now;
            if (dueTime.TotalMilliseconds <= 0)
            {
                runAt = GetNewNextDayDateRegisterTo();
                dueTime = runAt - DateTime.Now;
            }

            _timer = new Timer(dueTime.TotalMilliseconds);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
        {
            _timer?.Stop();
            _logger.Information("Run handler trading hour by timer");
            HandlingStartTradeHours();
            _logger.Information("Timer reset");
            SetTimerJobTradingHour();
        }
    }


    private void RegisterCandleForNewDate(DateTime date)
    {
        using (LogContext.PushProperty("Timeframe", Timeframe))
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

    [ExcludeFromCodeCoverage]
    protected virtual void Dispose(bool disposing)
    {
        _timer?.Dispose();
    }
}