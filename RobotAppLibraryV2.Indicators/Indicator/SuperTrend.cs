using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class SuperTrend : BaseIndicator<SuperTrendResult>
{
    public SuperTrend(int loopBackPeriodRequested = 14)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    protected override IEnumerable<SuperTrendResult> Update(IEnumerable<Candle> data)
    {
        return data.GetSuperTrend(LoopBackPeriod);
    }
}