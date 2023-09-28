using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.Enum;
using RobotAppLibraryV2.Result;
using Serilog;

namespace RobotAppLibraryV2.MoneyManagement;

// TODO : Faire evol les commandes en static pour le calculator.
public class MoneyManagement : IDisposable
{
    private const int StandardLotSize = 100000;

    private const string BaseSymbolAccount = "EUR";

    private readonly IApiHandler _apiHandler;

    private readonly ILogger? _logger;

    public readonly string PositionReference;

    public readonly StrategyResult StrategyResult = new();

    private AccountBalance _accountBalance = new();

    private Tick _tickPriceMain = new();

    public MoneyManagement(IApiHandler apiHandler, string symbol, ILogger? logger, string positionReference)
    {
        _apiHandler = apiHandler;
        PositionReference = positionReference;
        _logger = logger?.ForContext<MoneyManagement>();
        LotValueCalculator = new LotValueCalculator(apiHandler, logger, symbol);
        Init(symbol);
    }

    public LotValueCalculator LotValueCalculator { get; }

    public double MaxLot { get; private set; }
    public SymbolInfo SymbolInfo { get; private set; } = null!;


    /// <summary>
    ///     In percentage.
    /// </summary>
    public double Risque { get; set; } = 2;

    public int LooseStreak { get; set; } = 10;
    public double ToleratedDrawnDown { get; set; } = 10;
    public bool SecureControlPosition { get; set; }


    public void Dispose()
    {
        LotValueCalculator.Dispose();
    }


    public event EventHandler<MoneyManagementTresholdType>? TreshHoldEvent;

    private void Init(string symbol)
    {
        try
        {
            var listPositions = _apiHandler.GetAllPositionsByCommentAsync(PositionReference).Result;
            StrategyResult.UpdateGlobalData(listPositions);
            _apiHandler.NewBalanceEvent += ApiHandlerOnOnNewBalanceEvent;
            _accountBalance = _apiHandler.GetBalanceAsync().Result;
            _apiHandler.PositionUpdatedEvent += ApiHandlerOnPositionUpdatedEvent;
            _apiHandler.PositionClosedEvent += ApiHandlerOnPositionClosedEvent;
            SymbolInfo = _apiHandler.GetSymbolInformationAsync(symbol).Result;
            _tickPriceMain = _apiHandler.GetTickPriceAsync(symbol).Result;
            UpdateMaxLot();
        }
        catch (Exception e)
        {
            _logger?.Error(e, "Can't initialize Money management");
            throw new MoneyManagementException("Can't initialize Money management", e);
        }
    }

    private void ApiHandlerOnPositionClosedEvent(object? sender, Position e)
    {
        StrategyResult.UpdateGlobalData(e);
        if (SecureControlPosition)
        {
            if (CheckDrawnDownTreshold())
            {
                TreshHoldEvent?.Invoke(this, MoneyManagementTresholdType.Drowdown);
                _logger?.Warning("Threshold event {TreshHold} : {DrownDown}", MoneyManagementTresholdType.Drowdown,
                    StrategyResult.Results.Drawndown);
            }

            else if (CheckLooseStreakTreshold())
            {
                TreshHoldEvent?.Invoke(this, MoneyManagementTresholdType.LooseStreak);
                _logger?.Warning("Threshold event {TreshHold} : {LooseStreakTreshOld}",
                    MoneyManagementTresholdType.LooseStreak, LooseStreak);
            }
            else if (CheckProfitFactorTreshold())
            {
                TreshHoldEvent?.Invoke(this, MoneyManagementTresholdType.Profitfactor);
                _logger?.Warning("Threshold event {TreshHold} : {ProfitFactor}",
                    MoneyManagementTresholdType.Profitfactor, StrategyResult.Results.ProfitFactor);
            }
        }
    }

    private async void ApiHandlerOnPositionUpdatedEvent(object? sender, Position e)
    {
        if (CheckPerteRisqueTreshold(e))
        {
            _logger?.Warning("Position : {EId} perte risque treshold reached : {Profit}", e.Id, e.Profit);
            TreshHoldEvent?.Invoke(this, MoneyManagementTresholdType.ProfitTreshHold);
            if (e.StatusPosition is not StatusPosition.WaitClose || e.StatusPosition is not StatusPosition.Close)
                await _apiHandler.ClosePositionAsync(_tickPriceMain.Bid.GetValueOrDefault(), e);
        }
    }

    private void ApiHandlerOnOnNewBalanceEvent(object? sender, AccountBalance e)
    {
        _accountBalance = e;
        UpdateMaxLot();
    }

    public bool CheckPerteRisqueTreshold(Position position)
    {
        double? posValue = _accountBalance.Balance * (Risque / 100);
        decimal? profit = position.Profit;
        if (posValue * -1 >= (double?)profit) return true;

        return false;
    }

    public bool CheckLooseStreakTreshold()
    {
        var selected = StrategyResult.Positions.TakeLast(LooseStreak).ToList();

        if (selected.Count == LooseStreak && selected.TrueForAll(x => x.Profit < 0)) return true;

        return false;
    }

    public bool CheckDrawnDownTreshold()
    {
        var drawndown = StrategyResult.Results.Drawndown;
        var drawDownTheorique = _accountBalance.Balance * (ToleratedDrawnDown / 100);

        if (drawndown > 0 && drawndown >= (decimal)drawDownTheorique) return true;

        return false;
    }

    public bool CheckProfitFactorTreshold()
    {
        var profitfactor = StrategyResult.Results.ProfitFactor;
        if (profitfactor is > 0 and <= 1) return true;

        return false;
    }

    private void UpdateMaxLot()
    {
        if (SymbolInfo.Category == Category.Indices)
        {
            if (SymbolInfo.Leverage == 0)
            {
                var marginPerLot = LotValueCalculator.LotValueStandard * (double)_tickPriceMain.Bid.GetValueOrDefault();
                MaxLot = Math.Round(_accountBalance.Balance / marginPerLot, 2);
            }
            else
            {
                var marginPerLot = LotValueCalculator.LotValueStandard *
                                   (double)_tickPriceMain.Bid.GetValueOrDefault() *
                                   (SymbolInfo.Leverage / 100);
                MaxLot = Math.Round(_accountBalance.Balance / marginPerLot, 2);
            }
        }
        else if (SymbolInfo.Category == Category.Forex)
        {
            // TODO : a vérifier cas leverage == 0
            double marginPerLot = StandardLotSize;
            if (SymbolInfo.Leverage != 0) marginPerLot = StandardLotSize * (SymbolInfo.Leverage / 100);

            if (SymbolInfo.Currency1 == BaseSymbolAccount)
                marginPerLot *= (double)_tickPriceMain.Bid.GetValueOrDefault();
            else
                marginPerLot *= (double)LotValueCalculator._tickPriceSecondary.GetValueOrDefault().Bid
                    .GetValueOrDefault();

            var maxLotSize = _accountBalance.Balance / marginPerLot;

            MaxLot = Math.Round(maxLotSize, 2);
        }
    }

    public double CalculatePositionSize(decimal entryPrice, decimal stopLossPrice)
    {
        double positionSize = 0;
        var riskMoney = Risque / 100 * _accountBalance.Equity.GetValueOrDefault();

        if (SymbolInfo.Category == Category.Forex)
        {
            var pipsRisk = Math.Abs(entryPrice - stopLossPrice) / (decimal)SymbolInfo.TickSize2;
            var riskValue = pipsRisk * (decimal)LotValueCalculator.LotValueStandard;
            positionSize = riskMoney / (double)riskValue;
            var margeRequired = positionSize * StandardLotSize * (SymbolInfo.Leverage / 100);
            if (margeRequired >= _accountBalance.MarginFree)
            {
                _logger?.Warning("Margin exceded : {MarginRequired} | {MarginFree}, switch lot size to minimun value",
                    margeRequired, _accountBalance.MarginFree);
                positionSize = SymbolInfo.LotMin ?? throw new InvalidOperationException("Lot min is not defined");
            }
        }
        else if (SymbolInfo.Category == Category.Indices)
        {
            positionSize = riskMoney /
                           (double)(Math.Abs(entryPrice - stopLossPrice) *
                                    (decimal)LotValueCalculator.LotValueStandard);
            var margeRequired = positionSize * LotValueCalculator.LotValueStandard *
                                (double)_tickPriceMain.Bid.GetValueOrDefault() *
                                (SymbolInfo.Leverage / 100);
            if (margeRequired >= _accountBalance.MarginFree)
            {
                _logger?.Warning("Margin exceded : {MarginRequired} | {MarginFree}, switch lot size to minimun value",
                    margeRequired, _accountBalance.MarginFree);
                positionSize = SymbolInfo.LotMin ?? throw new InvalidOperationException("Lot min is not defined");
            }
        }

        if (positionSize < SymbolInfo.LotMin)
            throw new MoneyManagementException($"Position size to little : {positionSize}");

        //TODO : setup max lot tcheck


        return Math.Round(positionSize, 2);
    }
}