using RobotAppLibraryV2.Indicators.Custom.CandleCode;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class CandleCodeIndicator : BaseIndicator<CandleCodeResult>
{
    protected override IEnumerable<CandleCodeResult> Update(IEnumerable<Candle> data)
    {
        return data.GetCandleCode();
    }
}