using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.MoneyManagement;

public class MoneyManagement : IMoneyManagement
{
    private readonly IApiHandler _apiHandler;

    private readonly ILogger? _logger;

    private AccountBalance? _accountBalance = new();


    public MoneyManagement(IApiHandler apiHandler, string symbol, ILogger? logger,
        ILotValueCalculator lotValueCalculator, string positionReference)
    {
        _apiHandler = apiHandler;
        _logger = logger?.ForContext<MoneyManagement>();
        LotValueCalculator = lotValueCalculator;
        PositionReference = positionReference;

        Init(symbol);
    }

    public string PositionReference { get; set; }

    public ILotValueCalculator LotValueCalculator { get; }

    public double MaxLot { get; private set; }
    public SymbolInfo SymbolInfo { get; private set; } = null!;

    public void Dispose()
    {
        LotValueCalculator.Dispose();
    }

    public double CalculatePositionSize(decimal entryPrice, decimal stopLossPrice, double risk)
    {
        double positionSize = 0;
        var riskMoney = risk / 100 * _accountBalance.Equity.GetValueOrDefault();

        if (SymbolInfo.Category == Category.Forex)
        {
            var tickSize = SymbolInfo.Symbol.Contains("JPY") ? 0.01m : 0.0001m;
            var pipsRisk = Math.Abs(entryPrice - stopLossPrice) / tickSize;
            var riskValue = pipsRisk * (decimal)LotValueCalculator.PipValueStandard;
            var positionSizeByRisk = riskMoney / (double)riskValue;

            //TODO : TU ICI !
            var maxPositionSizeByMargin = (double)(_accountBalance.Equity / LotValueCalculator.MarginPerLot);
            positionSize = Math.Min(positionSizeByRisk, maxPositionSizeByMargin);

            // Ajustement. 
            positionSize -= 0.01;
        }
        else
        {
            var stopLossPoints = Math.Abs(entryPrice - stopLossPrice);

            var lossPerStopLoss = LotValueCalculator.PipValueStandard * (double)stopLossPoints;

            var positionSizeByRisk = riskMoney / lossPerStopLoss;

            // TODO : Pq cette var ? 
            var requiredMarge = positionSizeByRisk * LotValueCalculator.MarginPerLot;

            var maxPositionSizeByMargin = (double)(_accountBalance.Equity / LotValueCalculator.MarginPerLot);

            positionSize = Math.Min(positionSizeByRisk, maxPositionSizeByMargin);
        }

        if (positionSize < SymbolInfo.LotMin)
            throw new MoneyManagementException($"Position size to little : {positionSize}");

        return Math.Round(positionSize, 2);
    }

    private void Init(string symbol)
    {
        try
        {
            _apiHandler.NewBalanceEvent += ApiHandlerOnOnNewBalanceEvent;
            _accountBalance = _apiHandler.GetBalanceAsync().Result;
            SymbolInfo = _apiHandler.GetSymbolInformationAsync(symbol).Result;
            UpdateMaxLot();
        }
        catch (Exception e)
        {
            _logger?.Error(e, "Can't initialize Money management");
            throw new MoneyManagementException("Can't initialize Money management", e);
        }
    }


    private void ApiHandlerOnOnNewBalanceEvent(object? sender, AccountBalance? e)
    {
        _accountBalance = e;
        UpdateMaxLot();
    }

    private void UpdateMaxLot()
    {
        MaxLot = Math.Round(_accountBalance.Balance / LotValueCalculator.MarginPerLot, 2);
    }
}