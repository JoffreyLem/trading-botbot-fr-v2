using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Indicators;

public interface IIndicator
{
    public Tick LastTick { get; set; }
    public void UpdateIndicator(List<Candle> candles);
}