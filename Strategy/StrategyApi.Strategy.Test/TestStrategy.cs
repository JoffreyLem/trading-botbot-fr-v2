using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;
using ILogger = Serilog.ILogger;

namespace StrategyApi.Strategy.Test;

[VersionStrategy("2")]
public class TestStrategy : StrategyImplementationBase
{
    public TestStrategy() 
    {
        RunOnTick = true;
        CloseOnTick = true;
    }

    public SarIndicator SarIndicator { get; set; } = new SarIndicator();

    protected override void Run()
    {
        TypePosition type = SarIndicator.IsBuy() ? TypePosition.Buy : TypePosition.Sell;
        decimal sl = (decimal)SarIndicator.Last().Sar;

        OpenPosition(type, CalculateStopLoss(100,type), CalculateTakeProfit(80, type));
        CanRun = false;
    }


    protected override bool ShouldClosePosition(Position position)
    {
        return false;
    }
}