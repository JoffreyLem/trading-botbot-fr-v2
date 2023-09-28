using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RobotAppLibraryV2.ApiHandler.Backtest;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Attributes;
using RobotAppLibraryV2.Indicators;
using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.Enum;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Utils;
using Serilog;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Strategy;

public class StrategyBase : IDisposable
{
    private readonly IApiHandler _apiHandler;
    private readonly CandleList.CandleList _history;
    private readonly object _lockTickEvent = new();

    private readonly ILogger _logger;
    private readonly MoneyManagement.MoneyManagement _moneyManagement;
    private readonly PositionHandler _positionHandler;

    public Modeles.Result BacktestResult = new();

    public StrategyBase(
        StrategyImplementationBase strategyImplementationBase,
        string symbol,
        Timeframe timeframe,
        Timeframe? timeframe2,
        IApiHandler apiHandler,
        ILogger logger)
    {
        try
        {
            Id = Guid.NewGuid().ToString();
            StrategyImplementation = strategyImplementationBase;
            Symbol = symbol;
            Timeframe = timeframe;
            Timeframe2 = timeframe2;

            _logger = logger.ForContext<StrategyBase>()
                .ForContext("StrategyName", GetType().Name)
                .ForContext("StrategyId", Id);

            _apiHandler = apiHandler;

            _history = new CandleList.CandleList(apiHandler, _logger, timeframe, symbol);
            _moneyManagement = new MoneyManagement.MoneyManagement(apiHandler, symbol, logger, StrategyIdPosition);
            _positionHandler = new PositionHandler(logger, apiHandler, symbol);
            Init();
        }
        catch (Exception e)
        {
            throw new StrategyException("Can't create strategy", e);
        }
    }

    public string StrategyName => GetType().Name;
    public string Version => GetType().GetCustomAttribute<VersionStrategyAttribute>()?.Version ?? "0.0.1";

    /// <summary>
    ///     Used for position definition comment.
    /// </summary>
    public string StrategyIdPosition => $"{StrategyName}-{Version}-{Symbol}-{Timeframe}";

    public string Id { get; }
    public string Symbol { get; }
    public Timeframe Timeframe { get; }
    public Timeframe? Timeframe2 { get; }

    public bool CanRun { get; set; } = true;

    // TODO : Add to truc
    public bool IsBacktestRunning { get; private set; }
    public DateTime? LastBacktestExecution { get; private set; } = new DateTime();
    public bool RunOnTick { get; set; }
    public bool UpdateOnTick { get; set; }
    public bool CloseOnTick { get; set; }
    public bool PositionInProgress => _positionHandler.PositionInProgress;
    public IReadOnlyCollection<Position> Positions => _moneyManagement.StrategyResult.Positions;

    public bool SecureControlPosition
    {
        get => _moneyManagement.SecureControlPosition;
        set => _moneyManagement.SecureControlPosition = value;
    }

    public Position? PositionOpened => _positionHandler.PositionOpened;
    public Modeles.Result Results => _moneyManagement.StrategyResult.Results;
    public Tick LastPrice => _history.LastPrice.GetValueOrDefault();
    public Candle LastCandle => _history[_history.Count - 2];

    public Candle CurrentCandle => _history.Last();

    private List<IIndicator> IndicatorsList { get; } = new();
    private List<IIndicator> IndicatorsList2 { get; } = new();

    protected int DefaultStopLoss
    {
        get => _positionHandler.DefaultSl;
        set => _positionHandler.DefaultSl = value;
    }

    protected int DefaultTakeProfit
    {
        get => _positionHandler.DefaultTp;
        set => _positionHandler.DefaultTp = value;
    }

    private StrategyImplementationBase StrategyImplementation { get; }

    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        _moneyManagement.Dispose();
        _history.Dispose();
        GC.SuppressFinalize(this);
    }

    public event EventHandler<StrategyReasonClosed>? StrategyClosed;
    public event EventHandler<Tick>? TickEvent;
    public event EventHandler<Candle>? CandleEvent;
    public event EventHandler<MoneyManagementTresholdType>? TresholdEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;


    private void Init()
    {
        _apiHandler.Disconnected += ApiOnDisconnected;
        _history.OnTickEvent += HistoryOnOnTickEvent;
        _history.OnCandleEvent += HistoryOnOnCandleEvent;
        _moneyManagement.TreshHoldEvent += MoneyManagementOnTreshHoldEvent;
        _apiHandler.PositionOpenedEvent += (_, position) => PositionOpenedEvent?.Invoke(this, position);
        _apiHandler.PositionUpdatedEvent += (_, position) => PositionUpdatedEvent?.Invoke(this, position);
        _apiHandler.PositionClosedEvent += (_, position) => PositionClosedEvent?.Invoke(this, position);
        _apiHandler.PositionRejectedEvent += (_, position) => PositionRejectedEvent?.Invoke(this, position);

        // Care about order call
        InitStrategyImplementation();
        InitIndicator();

        _apiHandler.SubscribePrice(Symbol);
    }

    private void MoneyManagementOnTreshHoldEvent(object? sender, MoneyManagementTresholdType e)
    {
        CanRun = false;
        TresholdEvent?.Invoke(this, e);
    }


    private void InitStrategyImplementation()
    {
        StrategyImplementation.History = _history;
        StrategyImplementation.LastPrice = LastPrice;
        StrategyImplementation.LastCandle = LastCandle;
        StrategyImplementation.CurrentCandle = CurrentCandle;
        StrategyImplementation.DefaultStopLoss = DefaultStopLoss;
        StrategyImplementation.DefaultTp = DefaultTakeProfit;
        StrategyImplementation.Logger = _logger;
        StrategyImplementation.RunOnTick = RunOnTick;
        StrategyImplementation.UpdateOnTick = UpdateOnTick;
        StrategyImplementation.CloseOnTick = CloseOnTick;
        StrategyImplementation.CanRun = CanRun;
        StrategyImplementation.CalculateStopLossFunc = CalculateStopLoss;
        StrategyImplementation.CalculateTakeProfitFunc = CalculateTakeProfit;
        StrategyImplementation.OpenPositionAction = OpenPosition;
    }

    private void InitIndicator()
    {
        var indicators = StrategyImplementation.GetType().GetProperties()
            .Where(p => typeof(IIndicator).IsAssignableFrom(p.PropertyType)).ToList();

        foreach (var propertyInfo in indicators)
            if (propertyInfo.GetValue(StrategyImplementation) is IIndicator indicator)
            {
                if (propertyInfo.GetCustomAttribute<IndicatorLongerTermAttribute>() != null)
                    IndicatorsList2.Add(indicator);
                else
                    IndicatorsList.Add(indicator);
            }
    }

    private void HistoryOnOnTickEvent(Tick tick)
    {
        // TODO : Perf tests et voir temporisation dans history event tick ?
        lock (_lockTickEvent)
        {
            UpdateIndicator();
            try
            {
                if (CanRun && RunOnTick && !_positionHandler.PositionInProgress)
                {
                    RunHandler();
                }
                else
                {
                    if (_positionHandler.PositionInProgress)
                    {
                        var currentPosition = _positionHandler.PositionOpened;

                        if (UpdateOnTick) UpdateHandler(currentPosition);

                        if (CloseOnTick) CloseHandler(currentPosition);
                    }
                }

                TickEvent?.Invoke(this, tick);
                CandleEvent?.Invoke(this, _history.LastOrDefault());
            }
            catch (Exception e)
            {
                CanRun = false;
                _logger.Error(e, "Erreur de traitement tick");
                CloseStrategy(StrategyReasonClosed.Error);
            }
        }
    }

    private void HistoryOnOnCandleEvent(Candle candle)
    {
        try
        {
            UpdateIndicator();
            if (CanRun && !RunOnTick && !_positionHandler.PositionInProgress)
            {
                RunHandler();
            }
            else
            {
                if (_positionHandler.PositionInProgress)
                {
                    var currentPosition = _positionHandler.PositionOpened;

                    if (!UpdateOnTick) UpdateHandler(currentPosition);

                    if (!CloseOnTick) CloseHandler(currentPosition);
                }
            }
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Erreur de traitement candle");
            CloseStrategy(StrategyReasonClosed.Error);
        }
    }

    private void UpdateIndicator()
    {
        try
        {
            //TODO : TU : check si le count des indicators augmente.
            var candles = _history.TakeLast(1000).ToList();

            foreach (var indicator in IndicatorsList) indicator.UpdateIndicator(candles);

            if (Timeframe2 is not null)
            {
                var candles2 = _history.Aggregate(Timeframe2.GetValueOrDefault().ToPeriodSize()).AsEnumerable()
                    .Select(x => new Candle()
                        .SetOpen(x.Open)
                        .SetHigh(x.High)
                        .SetLow(x.Low)
                        .SetClose(x.Close)
                        .SetDate(x.Date)).ToList();

                foreach (var indicator in IndicatorsList2) indicator.UpdateIndicator(candles2);
            }
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Impossible de mettre à jour les indicateurs");
            throw new StrategyException("Impossible de mettre à jour les indicateurs", e);
        }
    }


    protected void OpenPosition(TypePosition typePosition, decimal sl = 0,
        decimal tp = 0, long? expiration = 0,
        double? volume = null)
    {
        //TODO : TU ici sur volume.
        if (sl == 0)
        {
            volume ??= _moneyManagement.SymbolInfo.LotMin;
        }
        else
        {
            var entryPrice = typePosition == TypePosition.Buy
                ? _history.LastPrice.GetValueOrDefault().Ask.GetValueOrDefault()
                : _history.LastPrice.GetValueOrDefault().Bid.GetValueOrDefault();
            volume ??= _moneyManagement.CalculatePositionSize(entryPrice, sl);
        }

        _positionHandler
            .OpenPositionAsync(Symbol, typePosition, volume.GetValueOrDefault(), StrategyIdPosition, sl, tp, expiration)
            .GetAwaiter().GetResult();
    }

    public void CloseStrategy(StrategyReasonClosed strategyReasonClosed = StrategyReasonClosed.User)
    {
        try
        {
            _logger.Fatal("On Closing strategy for reason {Reason}", strategyReasonClosed);

            CanRun = false;

            if (strategyReasonClosed is StrategyReasonClosed.Api)
            {
                var trades = _apiHandler.GetCurrentTradesAsync().Result;

                foreach (var position in trades.Where(x => x.Symbol == Symbol)
                             .Where(x => x.StrategyId == StrategyIdPosition))
                {
                    var price = _apiHandler.GetTickPriceAsync(Symbol).Result;
                    var closeprice =
                        position.TypePosition == TypePosition.Buy ? price.Ask : price.Bid;
                    _apiHandler.ClosePositionAsync(closeprice.GetValueOrDefault(),
                        position).GetAwaiter().GetResult();
                }
            }

            _apiHandler.UnsubscribePrice(Symbol);
            StrategyClosed?.Invoke(this, strategyReasonClosed);
            Dispose();
            _logger.Fatal("Strategy closed");
        }

        catch (Exception e)
        {
            _logger.Fatal(e, "can't closing strategy");
            throw new StrategyException();
        }
    }

    private void RunHandler()
    {
        try
        {
            _logger.Information("Try run strategy");
            StrategyImplementation.RunInternal();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on run");
        }
    }


    private void UpdateHandler(Position? position)
    {
        try
        {
            if (position != null)
            {
                var positionClone = position.Clone();
                _logger.Information("Try strategy update position {Id}", positionClone.Id);
                if (StrategyImplementation.ShouldUpdatePositionInternal(position))
                {
                    _logger.Information("Position {Id} can be updated", position.Id);
                    _positionHandler.UpdatePositionAsync(positionClone).GetAwaiter().GetResult();
                }
                else
                {
                    _logger.Information("Position {Id} can't be updated", position.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on update");
        }
    }

    private void CloseHandler(Position? position)
    {
        try
        {
            if (position != null)
            {
                _logger.Information("Try strategy close position {Id} ", position.Id);
                if (StrategyImplementation.ShouldClosePositionInternal(position))
                {
                    _logger.Information("Position {Id} can be closed", position.Id);
                    _positionHandler.ClosePositionAsync(position).GetAwaiter().GetResult();
                }
                else
                {
                    _logger.Information("Position {Id} can't be closed", position.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on close");
        }
    }

    public async Task LaunchBacktest(SpreadSimulator spreadSimulator)
    {
        try
        {
            IsBacktestRunning = true;
            var backtestApiHandler =
                new BacktestApiHandler(_apiHandler, spreadSimulator, _logger, new AccountBalance(), Symbol);
            var strategyToBacktest = new StrategyBase(StrategyImplementation, Symbol, Timeframe,
                Timeframe2, backtestApiHandler, _logger);

            strategyToBacktest.SecureControlPosition = false;
            strategyToBacktest.CanRun = true;

            await backtestApiHandler.StartAsync(Symbol, Timeframe);

            BacktestResult = strategyToBacktest.Results;
            LastBacktestExecution = DateTime.Now;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on backtest");
        }
        finally
        {
            IsBacktestRunning = false;
        }
    }

    protected decimal CalculateStopLoss(decimal pips, TypePosition typePosition)
    {
        return _positionHandler.CalculateStopLoss(pips, typePosition);
    }

    protected decimal CalculateTakeProfit(decimal pips, TypePosition typePosition)
    {
        return _positionHandler.CalculateTakeProfit(pips, typePosition);
    }

    private void ApiOnDisconnected(object? sender, EventArgs e)
    {
        _logger.Information("Api disconnected");
        CloseStrategy(StrategyReasonClosed.Api);
    }
}