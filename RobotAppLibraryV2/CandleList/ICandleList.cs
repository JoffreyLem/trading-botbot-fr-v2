using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.CandleList;

public interface ICandleList : IList<Candle>
{
    Tick? LastPrice { get; }
    void Dispose();
    event Action<Tick>? OnTickEvent;
    event Action<Candle>? OnCandleEvent;
    public IEnumerable<Candle> Aggregate(Timeframe timeframeData);
}