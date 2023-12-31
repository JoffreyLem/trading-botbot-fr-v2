using RobotAppLibraryV2.CandleList;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.Exposition;

/// <summary>
///     Only usable in strategy base.
/// </summary>
public abstract class StrategyImplementationBase
{
    public Func<decimal, TypeOperation, decimal> CalculateStopLossFunc;
    public Func<decimal, TypeOperation, decimal> CalculateTakeProfitFunc;

    public ILogger Logger;
    public Func<TypeOperation, decimal, decimal, long?, double?, double, Task> OpenPositionAction { get; set; }
    public string Name => GetType().Name;

    public abstract string? Version { get; }
    public ICandleList History { get; set; }

    public bool CanRun { get; set; } = true;
    public Tick LastPrice { get; set; }
    public Candle LastCandle { get; set; }

    public Candle CurrentCandle { get; set; }

    public int DefaultStopLoss { get; set; } = 50;
    public int DefaultTp { get; set; } = 50;
    public bool RunOnTick { get; set; } = false;
    public bool UpdateOnTick { get; set; } = false;
    public bool CloseOnTick { get; set; } = false;

    public abstract void Run();


    public async Task OpenPositionAsync(TypeOperation typePosition, decimal sl = 0, decimal tp = 0,
        long? expiration = 0, double? volume = null, double risk = 5)
    {
        await OpenPositionAction.Invoke(typePosition, sl, tp, expiration, volume, risk);
    }

    public void OpenPosition(TypeOperation typePosition, decimal sl = 0, decimal tp = 0,
        long? expiration = 0, double? volume = null, double risk = 5)
    {
        OpenPositionAction.Invoke(typePosition, sl, tp, expiration, volume, risk).GetAwaiter().GetResult();
    }

    public decimal CalculateStopLoss(decimal pips, TypeOperation typePosition)
    {
        return CalculateStopLossFunc.Invoke(pips, typePosition);
    }

    public decimal CalculateTakeProfit(decimal pips, TypeOperation typePosition)
    {
        return CalculateTakeProfitFunc.Invoke(pips, typePosition);
    }


    public virtual bool ShouldUpdatePosition(Position position)
    {
        return false;
    }


    public virtual bool ShouldClosePosition(Position position)
    {
        return false;
    }
}