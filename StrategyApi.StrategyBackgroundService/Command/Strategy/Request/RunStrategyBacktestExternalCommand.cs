using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Request;

public class RunStrategyBacktestExternalCommand : RunStrategyBacktestCommand
{
    public StrategyTypeEnum StrategyType { get; set; }
    public string Symbol { get; set; }
    public Timeframe Timeframe { get; set; }
    public Timeframe? Timeframe2 { get; set; }
}