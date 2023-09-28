using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class Macd : BaseIndicator<MacdResult>
{
    public Macd(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        FastPeriod = fastPeriod;
        SlowPeriod = slowPeriod;
        SignalPeriod = signalPeriod;
    }

    public int FastPeriod { get; set; }
    public int SlowPeriod { get; set; }
    public int SignalPeriod { get; set; }

    protected override List<MacdResult> Update(List<Candle> data)
    {
        return data.GetMacd().ToList();
    }
}