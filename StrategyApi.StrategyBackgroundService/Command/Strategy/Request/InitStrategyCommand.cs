using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Request;

public class InitStrategyCommand : ServiceCommandBaseStrategy<AcknowledgementResponse>
{
    public StrategyFileDto StrategyFileDto { get; set; }
    public string Symbol { get; set; }
    public Timeframe Timeframe { get; set; }
    public Timeframe? timeframe2 { get; set; }
}