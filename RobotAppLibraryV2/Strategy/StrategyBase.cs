using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.CandleList;
using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Indicators;
using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.events;
using RobotAppLibraryV2.MoneyManagement;
using RobotAppLibraryV2.PositionHandler;
using RobotAppLibraryV2.Results;
using Serilog;

namespace RobotAppLibraryV2.Strategy;

public sealed class StrategyBase : IDisposable
{
    private readonly IApiHandler _apiHandler;
    private readonly object _lockRunHandler = new();

    private readonly ILogger _logger;
    private readonly IMoneyManagement _moneyManagement;
    private readonly IPositionHandler _positionHandler;
    private readonly IStrategyResult _strategyResult;
    public readonly BackTest.BackTest BackTest;
    public readonly ICandleList History;

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
            BackTest = new BackTest.BackTest(strategyImplementationBase, apiHandler, logger, symbol, timeframe,
                timeframe2);
            Init();
        }
        catch (Exception e)
        {
            _logger?.Error(e, "Can't initialize strategy");
            throw new StrategyException("Can't create strategy", e);
        }
    }

    public string StrategyName => StrategyImplementation.Name;
    public string Version => StrategyImplementation.Version ?? "NotDefined";

    /// <summary>
    ///     Used for position definition comment.
    /// </summary>
    public string StrategyIdPosition => $"{StrategyName}-{Version}-{Symbol}-{Timeframe}";

    public string Id { get; }
    public string Symbol { get; }
    public Timeframe Timeframe { get; }
    public Timeframe? Timeframe2 { get; }

    // TODO : Gérer le set en fonction du StrategyDisabled
    public bool CanRun
    {
        get => StrategyImplementation.CanRun;
        set => StrategyImplementation.CanRun = value;
    }

    public bool StrategyDisabled { get; set; }

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

    public IReadOnlyCollection<Position> PositionsClosed => _strategyResult.Positions;

    public bool SecureControlPosition
    {
        get => _strategyResult.SecureControlPosition;
        set => _strategyResult.SecureControlPosition = value;
    }

    public Position? PositionOpened => _positionHandler.PositionOpened;
    public Result Results => _strategyResult.Results;
    public Tick LastPrice => History.LastPrice.GetValueOrDefault();
    public Candle? LastCandle => History.Count >= 2 ? History[^2] : null;

    public Candle? CurrentCandle => History.LastOrDefault();

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
        GC.SuppressFinalize(this);
    }

    public event EventHandler<RobotEvent<string>>? StrategyDisabledEvent;
    public event EventHandler<RobotEvent<Tick>>? TickEvent;
    public event EventHandler<RobotEvent<Candle>>? CandleEvent;
    public event EventHandler<RobotEvent<Position>>? PositionOpenedEvent;
    public event EventHandler<RobotEvent<Position>>? PositionUpdatedEvent;
    public event EventHandler<RobotEvent<Position>>? PositionRejectedEvent;
    public event EventHandler<RobotEvent<Position>>? PositionClosedEvent;
    public event EventHandler<RobotEvent<string>>? StrategyEvent;


    private void Init()
    {
        _apiHandler.Disconnected += ApiOnDisconnected;
        History.OnTickEvent += HistoryOnOnTickEvent;
        History.OnCandleEvent += HistoryOnOnCandleEvent;
        _strategyResult.ResultTresholdEvent += MoneyManagementOnTreshHoldEvent;
        _apiHandler.PositionOpenedEvent += (_, position) =>
            PositionOpenedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));
        _apiHandler.PositionUpdatedEvent += (_, position) =>
            PositionUpdatedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));
        _apiHandler.PositionClosedEvent += (_, position) =>
            PositionClosedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));
        _apiHandler.PositionRejectedEvent += (_, position) =>
            PositionRejectedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));

        // Care about order call
        InitStrategyImplementation();
        InitIndicator();

        _apiHandler.SubscribePrice(Symbol);
    }

    private void MoneyManagementOnTreshHoldEvent(object? sender, EventTreshold e)
    {
        DisableStrategy(StrategyReasonDisabled.Treshold).GetAwaiter().GetResult();
    }

    public async Task RunBackTest(double balance, decimal minSpread, decimal maxSpread)
    {
        await BackTest.RunBackTest(balance, minSpread, maxSpread);
    }


    private void InitStrategyImplementation()
    {
        // TODO : voir pour gérer l'update des valeurs ici ? 
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
        StrategyImplementation.CanRun = true;
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

    private async Task HistoryOnOnTickEvent(Tick tick)
    {
        UpdateIndicator();
        // TODO : Tester ce truc !!!!
        StrategyImplementation.LastPrice = tick;
        try
        {
            if (CanRun && RunOnTick && !_positionHandler.PositionInProgress)
            {
                lock (_lockRunHandler)
                {
                    RunHandler();
                }
            }
            else
            {
                // TODO : Faut refacto au propre ! 
                if (_positionHandler.PositionOpened is not null)
                {
                    var currentPosition = _positionHandler.PositionOpened;

                    if (UpdateOnTick) await UpdateHandler(currentPosition);

                    if (CloseOnTick) await CloseHandler(currentPosition);
                }
            }


            TickEvent?.Invoke(this, new RobotEvent<Tick>(tick, Id));
            CandleEvent?.Invoke(this, new RobotEvent<Candle>(History.LastOrDefault(), Id));
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Erreur de traitement tick");
            await DisableStrategy(StrategyReasonDisabled.Error, e);
        }
    }

    private async Task HistoryOnOnCandleEvent(Candle candle)
    {
        try
        {
            UpdateIndicator();
            if (CanRun && !RunOnTick && !_positionHandler.PositionInProgress)
            {
                lock (_lockRunHandler)
                {
                    RunHandler();
                }
            }
            else
            {
                // TODO : Faut refacto au propre ! 
                if (_positionHandler.PositionOpened is not null)
                {
                    var currentPosition = _positionHandler.PositionOpened;

                    if (!UpdateOnTick) await UpdateHandler(currentPosition);

                    if (!CloseOnTick) await CloseHandler(currentPosition);
                }
            }
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Erreur de traitement candle");
            await DisableStrategy(StrategyReasonDisabled.Error, e);
        }
    }

    // TODO : Faire tests de perf sur cette implémentation !
    private void UpdateIndicator()
    {
        try
        {
            var candles = History.TakeLast(1000).ToList();

            Parallel.ForEach(IndicatorsList, indicator => { indicator.UpdateIndicator(candles); });

            candles.Clear();
            candles = null;

            if (Timeframe2 is not null)
            {
                var candles2 = History.Aggregate(Timeframe2.GetValueOrDefault()).ToList();
                Parallel.ForEach(IndicatorsList2, indicator => { indicator.UpdateIndicator(candles2); });
                candles2.Clear();
                candles2 = null;
            }
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Impossible de mettre à jour les indicateurs");
            throw new StrategyException("Impossible de mettre à jour les indicateurs", e);
        }
    }


    private Task OpenPosition(TypeOperation typePosition, decimal sl = 0,
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

        return _positionHandler
            .OpenPositionAsync(Symbol, typePosition, volume.GetValueOrDefault(), sl, tp, expiration);
    }

    public async Task DisableStrategy(StrategyReasonDisabled strategyReasonDisabled, Exception? ex = null)
    {
        _logger.Fatal(ex, "On disabling strategy for reason {Reason}", strategyReasonDisabled);
        CanRun = false;
        StrategyDisabled = true;

        try
        {
            if (strategyReasonDisabled is StrategyReasonDisabled.User)
            {
                _apiHandler.UnsubscribePrice(Symbol);
                var trades = _apiHandler.GetCurrentTradeAsync(StrategyIdPosition).Result;
                if (trades is not null) await _positionHandler.ClosePositionAsync(trades);
            }

            _logger.Fatal("Strategy disabled");
        }
        catch (Exception e)
        {
            _logger.Fatal(e, "Can't completly disable strategy, some action don't work");
            throw new StrategyException();
        }
        finally
        {
            var disableMessage =
                $"The strategy {StrategyName}-{Symbol}-{Timeframe} have been disabled, cause of {strategyReasonDisabled}";
            StrategyDisabledEvent?.Invoke(this, new RobotEvent<string>(disableMessage, Id));
        }
    }

    private void RunHandler()
    {
        try
        {
            _logger.Verbose("Try run strategy");
            StrategyImplementation.Run();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on run");
        }
    }


    private async Task UpdateHandler(Position? position)
    {
        try
        {
            if (position != null)
            {
                var positionClone = position.Clone();
                _logger.Verbose("Try strategy update position {Id}", positionClone.Id);
                if (StrategyImplementation.ShouldUpdatePosition(positionClone))
                {
                    if (positionClone.StopLoss is 0) positionClone.StopLoss = position.StopLoss;

                    if (positionClone.TakeProfit is 0) positionClone.TakeProfit = position.TakeProfit;

                    _logger.Verbose("Position {Id} can be updated | New Sl {Sl} New Tp {Tp}", positionClone.Id,
                        positionClone.StopLoss, positionClone.TakeProfit);
                    await _positionHandler.UpdatePositionAsync(positionClone);
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

    private async Task CloseHandler(Position? position)
    {
        try
        {
            if (position != null)
            {
                _logger.Verbose("Try strategy close position {Id} ", position.Id);
                if (StrategyImplementation.ShouldClosePosition(position))
                {
                    _logger.Verbose("Position {Id} can be closed", position.Id);
                    await _positionHandler.ClosePositionAsync(position);
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
        DisableStrategy(StrategyReasonDisabled.Api).GetAwaiter().GetResult();
    }
}