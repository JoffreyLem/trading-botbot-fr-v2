using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class EmaIndicator : BaseIndicator<EmaResult>
{
    public EmaIndicator(int loopBackPeriodRequested = 20)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    protected override IEnumerable<EmaResult> Update(IEnumerable<Candle> data)
    {
        return data.GetEma(LoopBackPeriod);
    }
}