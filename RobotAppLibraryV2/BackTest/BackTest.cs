using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;
using Serilog;

namespace RobotAppLibraryV2.BackTest;

public class BackTest
{
    private StrategyBase? _strategyBase;

    public bool BacktestRunning = false;

    public DateTime? LastBacktestExecution;

    public Modeles.Result Result = new();

    public BackTest(StrategyImplementationBase strategyImplementationBase, IApiHandler apihandlerProxy, ILogger logger,
        string symbol, Timeframe timeframe, Timeframe? timeframe2)
    {
        StrategyImplementationBase = strategyImplementationBase;
        ApihandlerProxy = apihandlerProxy;
        Logger = logger;
        Symbol = symbol;
        Timeframe = timeframe;
        Timeframe2 = timeframe2;
    }

    private StrategyImplementationBase StrategyImplementationBase { get; }
    private IApiHandler ApihandlerProxy { get; set; }
    private ILogger Logger { get; set; }
    private string Symbol { get; set; }
    private Timeframe Timeframe { get; set; }
    private Timeframe? Timeframe2 { get; set; }

    public async Task RunBackTest(double balance, decimal minSpread, decimal maxSpread)
    {
        try
        {
            LastBacktestExecution = DateTime.Now;
            BacktestRunning = true;
            var backTestParameters =
                new BacktestParameters(Symbol, Timeframe, balance, minSpread, maxSpread);
            using var apiHandler =
                new BacktestApiHandler(new BackTestApiExecutor(ApihandlerProxy, Logger, backTestParameters),
                    Logger.ForContext<BackTest>());
            apiHandler.Disconnected += BackTestEnd;
            _strategyBase = new StrategyBase(StrategyImplementationBase, Symbol, Timeframe, Timeframe2, apiHandler,
                Logger, new StrategyServiceFactory());
            await apiHandler.StartBacktest();
        }
        catch (Exception e)
        {
            LastBacktestExecution = null;
            BacktestRunning = false;
            Logger.Error(e, "Cant initialize backtest");
        }
    }

    private void BackTestEnd(object? sender, EventArgs e)
    {
        BacktestRunning = false;
        Result = new()
        {
            DrawndownMax = _strategyBase.Results.DrawndownMax,
            Drawndown = _strategyBase.Results.Drawndown,
            GainMax = _strategyBase.Results.GainMax,
            MoyenneNegative = _strategyBase.Results.MoyenneNegative,
            MoyennePositive = _strategyBase.Results.MoyennePositive,
            MoyenneProfit = _strategyBase.Results.MoyenneProfit,
            PerteMax = _strategyBase.Results.PerteMax,
            Profit = _strategyBase.Results.Profit,
            ProfitFactor = _strategyBase.Results.ProfitFactor,
            ProfitNegatif = _strategyBase.Results.ProfitNegatif,
            ProfitPositif = _strategyBase.Results.ProfitPositif,
            RatioMoyennePositifNegatif = _strategyBase.Results.RatioMoyennePositifNegatif,
            TauxReussite = _strategyBase.Results.TauxReussite,
            TotalPositionNegative = _strategyBase.Results.TotalPositionNegative,
            TotalPositionPositive = _strategyBase.Results.TotalPositionPositive,
            TotalPositions = _strategyBase.Results.TotalPositions
        };
        _strategyBase.Dispose();
        _strategyBase = null;
    }
}