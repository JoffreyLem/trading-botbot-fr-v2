using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Attributes;
using RobotAppLibraryV2.CandleList;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Indicators;
using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.MoneyManagement;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Result;
using Serilog;

namespace RobotAppLibraryV2.Strategy;

public sealed class StrategyBase : IStrategyEvent, IDisposable
{
    private readonly IApiHandler _apiHandler;
    private readonly object _lockOpenPositionEvent = new();
    private readonly object _lockTickEvent = new();

    private readonly ILogger _logger;
    private readonly IMoneyManagement _moneyManagement;
    private readonly IPositionHandler _positionHandler;
    private readonly IStrategyResult _strategyResult;
    public readonly ICandleList History;

    private bool canRun = true;

    public StrategyBase(
        StrategyImplementationBase strategyImplementationBase,
        string symbol,
        Timeframe timeframe,
        Timeframe? timeframe2,
        IApiHandler apiHandler,
        ILogger logger,
        IStrategyServiceFactory strategyServiceFactory)
    {
        try
        {
            Id = Guid.NewGuid().ToString();
            StrategyImplementation = strategyImplementationBase;
            Symbol = symbol;
            Timeframe = timeframe;
            Timeframe2 = timeframe2;

            _logger = logger.ForContext<StrategyBase>()
                .ForContext("StrategyName", StrategyImplementation.GetType().Name)
                .ForContext("StrategyId", Id);

            _apiHandler = apiHandler;

            History = strategyServiceFactory.GetHistory(logger, apiHandler, symbol, timeframe);
            _moneyManagement =
                strategyServiceFactory.GetMoneyManagement(apiHandler, logger, symbol, StrategyIdPosition);
            _strategyResult = strategyServiceFactory.GetStrategyResultService(apiHandler, StrategyIdPosition);
            _positionHandler =
                strategyServiceFactory.GetPositionHandler(logger, apiHandler, symbol, StrategyIdPosition);
            Init();
        }
        catch (Exception e)
        {
            _logger?.Error(e, "Can't initialize strategy");
            throw new StrategyException("Can't create strategy", e);
        }
    }

    public string StrategyName => StrategyImplementation.Name;
    public string Version => GetType().GetCustomAttribute<VersionStrategyAttribute>()?.Version ?? "NotDefined";


    /// <summary>
    ///     Used for position definition comment.
    /// </summary>
    public string StrategyIdPosition => $"{StrategyName}-{Version}-{Symbol}-{Timeframe}";

    public string Id { get; }
    public string Symbol { get; }
    public Timeframe Timeframe { get; }
    public Timeframe? Timeframe2 { get; }

    public bool CanRun
    {
        get => StrategyImplementation.CanRun;
        set
        {
            StrategyImplementation.CanRun = value;
            StrategyInfoUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool RunOnTick
    {
        get => StrategyImplementation.RunOnTick;
        set => StrategyImplementation.RunOnTick = value;
    }

    public bool UpdateOnTick
    {
        get => StrategyImplementation.UpdateOnTick;
        set => StrategyImplementation.UpdateOnTick = value;
    }

    public bool CloseOnTick
    {
        get => StrategyImplementation.CloseOnTick;
        set => StrategyImplementation.CloseOnTick = value;
    }

    public bool PositionInProgress => _positionHandler.PositionInProgress;
    public IReadOnlyCollection<Position> PositionsClosed => _strategyResult.Positions;

    public bool SecureControlPosition
    {
        get => _strategyResult.SecureControlPosition;
        set => _strategyResult.SecureControlPosition = value;
    }

    public Position? PositionOpened => _positionHandler.PositionOpened;
    public Modeles.Result Results => _strategyResult.Results;
    public Tick LastPrice => History.LastPrice.GetValueOrDefault();
    public Candle LastCandle => History[^2];

    public Candle CurrentCandle => History.Last();

    private List<IIndicator> IndicatorsList { get; } = new();
    private List<IIndicator> IndicatorsList2 { get; } = new();

    public int DefaultStopLoss
    {
        get => _positionHandler.DefaultSl;
        set
        {
            _positionHandler.DefaultSl = value;
            StrategyImplementation.DefaultStopLoss = value;
        }
    }

    public int DefaultTakeProfit
    {
        get => _positionHandler.DefaultTp;
        set
        {
            _positionHandler.DefaultTp = value;
            StrategyImplementation.DefaultTp = value;
        }
    }

    private StrategyImplementationBase StrategyImplementation { get; }

    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        _moneyManagement.Dispose();
        History.Dispose();
        GC.SuppressFinalize(this);
    }

    public event EventHandler<StrategyReasonClosed>? StrategyClosed;
    public event EventHandler<Tick>? TickEvent;
    public event EventHandler<Candle>? CandleEvent;
    public event EventHandler<EventTreshold>? TresholdEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;

    public event EventHandler? StrategyInfoUpdated;


    private void Init()
    {
        _apiHandler.Disconnected += ApiOnDisconnected;
        History.OnTickEvent += HistoryOnOnTickEvent;
        History.OnCandleEvent += HistoryOnOnCandleEvent;
        _strategyResult.ResultTresholdEvent += MoneyManagementOnTreshHoldEvent;
        _apiHandler.PositionOpenedEvent += (_, position) => PositionOpenedEvent?.Invoke(this, position);
        _apiHandler.PositionUpdatedEvent += (_, position) => PositionUpdatedEvent?.Invoke(this, position);
        _apiHandler.PositionClosedEvent += (_, position) => PositionClosedEvent?.Invoke(this, position);
        _apiHandler.PositionRejectedEvent += (_, position) => PositionRejectedEvent?.Invoke(this, position);

        // Care about order call
        InitStrategyImplementation();
        InitIndicator();

        _apiHandler.SubscribePrice(Symbol);
    }

    private void MoneyManagementOnTreshHoldEvent(object? sender, EventTreshold e)
    {
        CanRun = false;
        TresholdEvent?.Invoke(this, e);
    }


    private void InitStrategyImplementation()
    {
        StrategyImplementation.History = History;
        StrategyImplementation.LastPrice = LastPrice;
        StrategyImplementation.LastCandle = LastCandle;
        StrategyImplementation.CurrentCandle = CurrentCandle;
        StrategyImplementation.Logger = _logger;
        StrategyImplementation.CalculateStopLossFunc = CalculateStopLoss;
        StrategyImplementation.CalculateTakeProfitFunc = CalculateTakeProfit;
        StrategyImplementation.OpenPositionAction = OpenPosition;
        DefaultStopLoss = StrategyImplementation.DefaultStopLoss;
        DefaultTakeProfit = StrategyImplementation.DefaultTp;
        RunOnTick = StrategyImplementation.RunOnTick;
        UpdateOnTick = StrategyImplementation.UpdateOnTick;
        CloseOnTick = StrategyImplementation.CloseOnTick;
        CanRun = StrategyImplementation.CanRun;
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

        UpdateIndicator();
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
                CandleEvent?.Invoke(this, History.LastOrDefault());
            }
            catch (Exception e)
            {
                // TODO : Catch peut être inutile ? 
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
            var candles = History.TakeLast(1000);

            foreach (var indicator in IndicatorsList) indicator.UpdateIndicator(candles);

            if (Timeframe2 is not null)
            {
                var candles2 = History.Aggregate(Timeframe2.GetValueOrDefault());

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


    private void OpenPosition(TypeOperation typePosition, decimal sl = 0,
        decimal tp = 0, long? expiration = 0,
        double? volume = null, double risk = 5)
    {
        if (sl == 0)
        {
            volume ??= _moneyManagement.SymbolInfo.LotMin;
        }
        else
        {
            var entryPrice = typePosition == TypeOperation.Buy
                ? History.LastPrice.GetValueOrDefault().Ask.GetValueOrDefault()
                : History.LastPrice.GetValueOrDefault().Bid.GetValueOrDefault();
            volume ??= _moneyManagement.CalculatePositionSize(entryPrice, sl, risk);
        }

        _positionHandler
            .OpenPositionAsync(Symbol, typePosition, volume.GetValueOrDefault(), sl, tp, expiration)
            .GetAwaiter().GetResult();
    }

    public void CloseStrategy(StrategyReasonClosed strategyReasonClosed)
    {
        try
        {
            _logger.Fatal("On Closing strategy for reason {Reason}", strategyReasonClosed);

            CanRun = false;

            if (strategyReasonClosed is StrategyReasonClosed.User)
            {
                var trades = _apiHandler.GetCurrentTradeAsync(StrategyIdPosition).Result;

                if (trades is not null) _positionHandler.ClosePositionAsync(trades).GetAwaiter().GetResult();

                _apiHandler.UnsubscribePrice(Symbol);
            }


            StrategyClosed?.Invoke(this, strategyReasonClosed);
            StrategyClosed = null;
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
            StrategyImplementation.Run();
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
                _logger.Verbose("Try strategy update position {Id}", positionClone.Id);
                if (StrategyImplementation.ShouldUpdatePosition(position))
                {
                    _logger.Verbose("Position {Id} can be updated", position.Id);
                    _positionHandler.UpdatePositionAsync(positionClone).GetAwaiter().GetResult();
                }
                else
                {
                    _logger.Verbose("Position {Id} can't be updated", position.Id);
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
                _logger.Verbose("Try strategy close position {Id} ", position.Id);
                if (StrategyImplementation.ShouldClosePosition(position))
                {
                    _logger.Verbose("Position {Id} can be closed", position.Id);
                    _positionHandler.ClosePositionAsync(position).GetAwaiter().GetResult();
                }
                else
                {
                    _logger.Verbose("Position {Id} can't be closed", position.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on close");
        }
    }

    [ExcludeFromCodeCoverage]
    private decimal CalculateStopLoss(decimal pips, TypeOperation typePosition)
    {
        return _positionHandler.CalculateStopLoss(pips, typePosition);
    }

    [ExcludeFromCodeCoverage]
    private decimal CalculateTakeProfit(decimal pips, TypeOperation typePosition)
    {
        return _positionHandler.CalculateTakeProfit(pips, typePosition);
    }

    [ExcludeFromCodeCoverage]
    private void ApiOnDisconnected(object? sender, EventArgs e)
    {
        _logger.Information("Api disconnected");
        CloseStrategy(StrategyReasonClosed.Api);
    }
}