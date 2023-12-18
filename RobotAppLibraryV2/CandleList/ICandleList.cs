using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.CandleList;

public interface ICandleList : IList<Candle>
{
    Tick? LastPrice { get; }
    void Dispose();
    event Func<Tick, Task>? OnTickEvent;
    event Func<Candle, Task>? OnCandleEvent;
    public IEnumerable<Candle> Aggregate(Timeframe timeframeData);
}