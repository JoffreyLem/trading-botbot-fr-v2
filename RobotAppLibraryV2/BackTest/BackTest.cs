using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;
using Serilog;

namespace RobotAppLibraryV2.BackTest;

public class BackTest : IDisposable
{
    private StrategyBase? _strategyBase;

    public bool BacktestRunning;

    public DateTime? LastBacktestExecution;

    public Result? Result;

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
    private IApiHandler ApihandlerProxy { get; }
    private ILogger Logger { get; }
    private string Symbol { get; }
    private Timeframe Timeframe { get; }
    private Timeframe? Timeframe2 { get; }

    public void Dispose()
    {
        _strategyBase?.Dispose();
        _strategyBase = null;
        GC.Collect();
    }

    public async Task RunBackTest(double balance, decimal minSpread, decimal maxSpread)
    {
        try
        {
            var logger = Serilog.Core.Logger.None;
            StrategyImplementationBase.Logger = logger;
            LastBacktestExecution = DateTime.Now;
            BacktestRunning = true;
            var backTestParameters =
                new BacktestParameters(Symbol, Timeframe, balance, minSpread, maxSpread);
            using var apiHandler =
                new BacktestApiHandler(new BackTestApiExecutor(ApihandlerProxy, logger, backTestParameters),
                    logger);

            _strategyBase = new StrategyBase(StrategyImplementationBase, Symbol, Timeframe, Timeframe2, apiHandler,
                logger, new StrategyServiceFactory());
            StrategyImplementationBase.CanRun = true;
            await apiHandler.StartBacktest();
            BackTestEnd();
        }
        catch (Exception e)
        {
            LastBacktestExecution = null;
            BacktestRunning = false;
            Logger.Error(e, "Cant initialize backtest");
        }
    }

    private void BackTestEnd()
    {
        BacktestRunning = false;
        Result = new Result
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
        Dispose();
    }
}