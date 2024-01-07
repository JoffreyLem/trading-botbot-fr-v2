using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class CciIndicator : BaseIndicator<CciResult>
{
    public CciIndicator(int loopBackPeriodRequested = 20)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }


    protected override IEnumerable<CciResult> Update(IEnumerable<Candle> data)
    {
        return data.GetCci(LoopBackPeriod);
    }
}