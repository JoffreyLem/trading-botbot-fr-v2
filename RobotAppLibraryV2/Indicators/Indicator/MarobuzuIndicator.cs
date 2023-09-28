using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class MarobuzuIndicator : BaseIndicator<CandleResult>
{
    protected override List<CandleResult> Update(List<Candle> data)
    {
        return data.GetMarubozu().ToList();
    }
}