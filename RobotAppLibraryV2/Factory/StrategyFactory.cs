using System.Diagnostics.CodeAnalysis;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;
using Serilog;

namespace RobotAppLibraryV2.Factory;

[ExcludeFromCodeCoverage]
public static class StrategyFactory
{
    public static StrategyBase GenerateStrategy<T>(T strategyImplementationBase, string symbol, Timeframe timeframe,
        Timeframe? timeframe2, IApiHandler apiHandler, ILogger logger) where T : StrategyImplementationBase
    {
        var instanceStrategy = Activator.CreateInstance(typeof(T)) as StrategyImplementationBase;
        return new StrategyBase(instanceStrategy, symbol, timeframe, timeframe2, apiHandler, logger);
    }
}