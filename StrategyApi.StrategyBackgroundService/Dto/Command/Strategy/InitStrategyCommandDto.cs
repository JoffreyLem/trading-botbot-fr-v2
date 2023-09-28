using RobotAppLibraryV2.Modeles;
using StrategyApi.Dto.Enum;

namespace StrategyApi.StrategyBackgroundService.Dto.Command.Strategy;

public class InitStrategyCommandDto : StrategyCommandBaseDto
{
    public StrategyTypeEnum StrategyType { get; set; }
    public string Symbol { get; set; }
    public Timeframe Timeframe { get; set; }
    public Timeframe? timeframe2 { get; set; }
}