using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators;

public interface IIndicator
{
    public Tick LastTick { get; set; }
    public void UpdateIndicator(IEnumerable<Candle> candles);
}