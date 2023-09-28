using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class Slope : BaseIndicator<SlopeResult>
{
    public Slope(int loopBackPeriodRequested = 14)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    protected override List<SlopeResult> Update(List<Candle> data)
    {
        return data.GetSlope(LoopBackPeriod).ToList();
    }
}