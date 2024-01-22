using Robot.DataBase.Modeles;
using RobotAppLibraryV2.Modeles;

namespace Robot.Server.Command.Strategy.Request;

public class InitStrategyCommand : ServiceCommandBaseStrategy<AcknowledgementResponse>
{
    public StrategyFile StrategyFileDto { get; set; }
    public string Symbol { get; set; }
    public Timeframe Timeframe { get; set; }
    public Timeframe? timeframe2 { get; set; }
}