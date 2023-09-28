using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators;

public abstract class BaseIndicator<T> : List<T>, IIndicator where T : ResultBase
{
    protected BaseIndicator() : base(2100)
    {
    }

    public int LoopBackPeriod { get; set; } = 1;

    public Tick LastTick { get; set; } = new();

    public void UpdateIndicator(List<Candle> data)
    {
        if (data.Count > LoopBackPeriod)
        {
            Clear();
            var dataToAdd = Update(data);
            AddRange(dataToAdd);
        }
    }

    protected abstract List<T> Update(List<Candle> data);
}