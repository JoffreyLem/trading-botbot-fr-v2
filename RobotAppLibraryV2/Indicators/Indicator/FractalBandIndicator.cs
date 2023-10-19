using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class FractalBandIndicator : BaseIndicator<FcbResult>
{
    public FractalBandIndicator(int loopBackPeriodRequested = 20)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    protected override IEnumerable<FcbResult> Update(IEnumerable<Candle> data)
    {
        return data.GetFcb(LoopBackPeriod);
    }
}