using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class Rsi : BaseIndicator<RsiResult>
{
    public Rsi(int loopBackPeriodRequested = 14)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }


    protected override IEnumerable<RsiResult> Update(IEnumerable<Candle> data)
    {
        return data.GetRsi(LoopBackPeriod);
    }
}