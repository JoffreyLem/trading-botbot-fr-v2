using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class DojiIndicator : BaseIndicator<CandleResult>
{
    protected override List<CandleResult> Update(List<Candle> data)
    {
        return data.GetDoji().ToList();
    }
}