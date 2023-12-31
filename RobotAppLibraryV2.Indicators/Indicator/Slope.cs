using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class Slope : BaseIndicator<SlopeResult>
{
    public Slope(int loopBackPeriodRequested = 14)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    protected override IEnumerable<SlopeResult> Update(IEnumerable<Candle> data)
    {
        return data.GetSlope(LoopBackPeriod);
    }
}