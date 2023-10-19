using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class BollingerBand : BaseIndicator<BollingerBandsResult>
{
    public BollingerBand(int loopBackPeriodRequested = 20)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }


    protected override IEnumerable<BollingerBandsResult> Update(IEnumerable<Candle> data)
    {
        return data.GetBollingerBands(LoopBackPeriod);
    }
}