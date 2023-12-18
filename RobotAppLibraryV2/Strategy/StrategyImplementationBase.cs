using System.Runtime.CompilerServices;
using RobotAppLibraryV2.CandleList;
using RobotAppLibraryV2.Modeles;
using Serilog;

[assembly: InternalsVisibleTo("RobotAppLibraryV2.Tests")]

namespace RobotAppLibraryV2.Strategy;

/// <summary>
///     Only usable in strategy base.
/// </summary>
public abstract class StrategyImplementationBase
{
    // TODO : Implementer le property changed ici !
    internal Func<decimal, TypeOperation, decimal> CalculateStopLossFunc;
    internal Func<decimal, TypeOperation, decimal> CalculateTakeProfitFunc;


    protected internal ILogger Logger;
    internal Func<TypeOperation, decimal, decimal, long?, double?, double, Task> OpenPositionAction { get; set; }
    public string Name => GetType().Name;
    protected internal ICandleList History { get; set; }

    protected internal bool CanRun { get; set; } = true;
    protected internal Tick LastPrice { get; set; }

    protected internal Candle LastCandle { get; set; }

    protected internal Candle CurrentCandle { get; set; }

    protected internal int DefaultStopLoss { get; set; } = 50;
    protected internal int DefaultTp { get; set; } = 50;
    protected internal bool RunOnTick { get; set; } = false;
    protected internal bool UpdateOnTick { get; set; } = false;
    protected internal bool CloseOnTick { get; set; } = false;

    protected internal abstract void Run();


    protected async Task OpenPositionAsync(TypeOperation typePosition, decimal sl = 0, decimal tp = 0,
        long? expiration = 0, double? volume = null, double risk = 5)
    {
        await OpenPositionAction.Invoke(typePosition, sl, tp, expiration, volume, risk);
    }

    protected void OpenPosition(TypeOperation typePosition, decimal sl = 0, decimal tp = 0,
        long? expiration = 0, double? volume = null, double risk = 5)
    {
        OpenPositionAction.Invoke(typePosition, sl, tp, expiration, volume, risk).GetAwaiter().GetResult();
    }

    protected decimal CalculateStopLoss(decimal pips, TypeOperation typePosition)
    {
        return CalculateStopLossFunc.Invoke(pips, typePosition);
    }

    protected decimal CalculateTakeProfit(decimal pips, TypeOperation typePosition)
    {
        return CalculateTakeProfitFunc.Invoke(pips, typePosition);
    }


    protected internal virtual bool ShouldUpdatePosition(Position position)
    {
        return false;
    }


    protected internal virtual bool ShouldClosePosition(Position position)
    {
        return false;
    }
}