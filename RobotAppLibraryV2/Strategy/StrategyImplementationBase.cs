using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.Strategy;

/// <summary>
///     Only usable in strategy base.
/// </summary>
public abstract class StrategyImplementationBase
{

    public string Name => GetType().Name;
    
    internal Func<decimal, TypePosition, decimal> CalculateStopLossFunc;
    internal Func<decimal, TypePosition, decimal> CalculateTakeProfitFunc;

    protected internal ILogger Logger;
    protected internal List<Candle> History { get; set; }

    protected internal bool CanRun { get; set; }

    internal Action<TypePosition, decimal, decimal, long?, double?> OpenPositionAction { get; set; }
    protected internal Tick LastPrice { get; set; }

    protected internal Candle LastCandle { get; set; }

    protected internal Candle CurrentCandle { get; set; }

    protected internal int DefaultStopLoss { get; set; }
    protected internal int DefaultTp { get; set; }
    protected internal bool RunOnTick { get; set; }
    protected internal bool UpdateOnTick { get; set; }
    protected internal bool CloseOnTick { get; set; }

    protected abstract void Run();

    internal void RunInternal()
    {
        Run();
    }

    protected void OpenPosition(TypePosition typePosition, decimal sl = 0, decimal tp = 0,
        long? expiration = 0, double? volume = null)
    {
        OpenPositionAction?.Invoke(typePosition, sl, tp, expiration, volume);
    }

    protected decimal CalculateStopLoss(decimal pips, TypePosition typePosition)
    {
        return (decimal)CalculateStopLossFunc?.Invoke(pips, typePosition);
    }

    protected decimal CalculateTakeProfit(decimal pips, TypePosition typePosition)
    {
        return (decimal)CalculateTakeProfitFunc?.Invoke(pips, typePosition);
    }


    protected virtual bool ShouldUpdatePosition(Position position)
    {
        return false;
    }

    internal bool ShouldUpdatePositionInternal(Position position)
    {
        return ShouldUpdatePosition(position);
    }

    protected virtual bool ShouldClosePosition(Position position)
    {
        return false;
    }

    internal bool ShouldClosePositionInternal(Position position)
    {
        return ShouldClosePosition(position);
    }
}