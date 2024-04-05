using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;

namespace Robot.Server.Services.Template;

public class StrategyBaseTemplate : StrategyImplementationBase
{
    public StrategyBaseTemplate()
    {
        RunOnTick = true;
        CloseOnTick = true;
        CanRun = true;
    }

    public SarIndicator SarIndicator { get; set; } = new();

    public override string? Version => "7";

    public override void Run()
    {
        var type = SarIndicator.IsBuy() ? TypeOperation.Buy : TypeOperation.Sell;
        var sl = SarIndicator.Last().Sar;

        OpenPosition(type, CalculateStopLoss(100, type), CalculateTakeProfit(80, type));
    }


    public override bool ShouldClosePosition(Position position)
    {
        return true;
    }
}