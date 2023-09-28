using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class ForceIndex : BaseIndicator<ForceIndexResult>
{
    public ForceIndex(int loopBackPeriodRequested = 20)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    protected override List<ForceIndexResult> Update(List<Candle> data)
    {
        return data.GetForceIndex(LoopBackPeriod).ToList();
    }
}