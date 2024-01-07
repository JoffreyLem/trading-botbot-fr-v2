using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace StrategyApi.Strategy.NewMain;

public class NewMainStrategy : StrategyImplementationBase
{
    public NewMainStrategy()
    {
        UpdateOnTick = false;
        CloseOnTick = false;
    }

    public PivotPoint PivotPoint { get; set; } = new(PeriodSize.Day);

    public BollingerBand BollingerBand { get; set; } = new();

    public SuperTrend SuperTrend { get; set; } = new();
    [IndicatorLongerTerm] public SuperTrend B_SuperTrend { get; set; } = new();

    public Rsi Rsi { get; set; } = new();


    public override string? Version => "1";

    public override void Run()
    {
        var spt2 = B_SuperTrend.LastOrDefault();
        var rsi = Rsi.LastOrDefault().Rsi;
        if (LastPrice.Bid > spt2.SuperTrend && rsi < 70)
            TryOpenBuy();
        else if (LastPrice.Bid < spt2.SuperTrend && rsi > 30) TryOpenSell();
    }

    public override bool ShouldUpdatePosition(Position position)
    {
        if (position.TypePosition == TypeOperation.Buy)
            UpdatePositionBuyStopLoss(position);
        else if (position.TypePosition == TypeOperation.Sell) UpdatePositionSellStopLoss(position);
        return true;
    }

    public override bool ShouldClosePosition(Position position)
    {
        if (position.TypePosition == TypeOperation.Buy)
            return ClosePositionBuy(position);
        return ClosePositionSell(position);
        return base.ShouldClosePosition(position);
    }

    private bool ClosePositionBuy(Position position)
    {
        var rsi3 = Rsi[^3];
        var rsi2 = Rsi[^2];

        if (rsi3.Rsi > 70 && rsi2.Rsi <= 70) return true;

        return false;
    }

    private bool ClosePositionSell(Position position)
    {
        var rsi3 = Rsi[^3];
        var rsi2 = Rsi[^2];

        if (rsi3.Rsi < 30 && rsi2.Rsi >= 30) return true;

        return false;
    }

    private void UpdatePositionBuyStopLoss(Position position)
    {
        var lastPivotPoint = PivotPoint.LastOrDefault();
        var spt = SuperTrend[^2];
        var levels = new[] { lastPivotPoint.PP, lastPivotPoint.R1, lastPivotPoint.R2, lastPivotPoint.R3 };

        foreach (var level in levels)
            if (LastCandle.Close > level)
            {
                position.StopLoss = Math.Max(spt.SuperTrend.GetValueOrDefault(), level.GetValueOrDefault());
                break;
            }
    }

    private void UpdatePositionSellStopLoss(Position position)
    {
        var lastPivotPoint = PivotPoint.LastOrDefault();
        var spt = SuperTrend[^2];
        var levels = new[] { lastPivotPoint.PP, lastPivotPoint.R1, lastPivotPoint.R2, lastPivotPoint.R3 };

        foreach (var level in levels)
            if (LastCandle.Close < level)
            {
                position.StopLoss = Math.Min(spt.SuperTrend.GetValueOrDefault(), level.GetValueOrDefault());
                break;
            }
    }


    private void TryOpenBuy()
    {
        var lastPivotPoint = PivotPoint.LastOrDefault();
        var spt = SuperTrend.LastOrDefault();

        var candle3 = History[^3];
        var candle2 = History[^2];

        var crossConditionPivot = candle3.Close < lastPivotPoint.PP && candle2.Close > lastPivotPoint.PP;
        var crossConditionSpt = candle3.Close < spt.SuperTrend && candle2.Close > spt.SuperTrend;

        if ((LastPrice.Bid > lastPivotPoint.PP && crossConditionSpt) ||
            (LastPrice.Bid > spt.SuperTrend && crossConditionPivot))
            OpenPosition(TypeOperation.Buy,
                lastPivotPoint.PP.GetValueOrDefault(),
                lastPivotPoint.R3.GetValueOrDefault());
    }

    private void TryOpenSell()
    {
        var lastPivotPoint = PivotPoint.LastOrDefault();
        var spt = SuperTrend.LastOrDefault();

        var candle3 = History[^3];
        var candle2 = History[^2];

        var crossConditionPivot = candle3.Close > lastPivotPoint.PP && candle2.Close < lastPivotPoint.PP;
        var crossConditionSpt = candle3.Close > spt.SuperTrend && candle2.Close < spt.SuperTrend;

        if ((LastPrice.Bid < lastPivotPoint.PP && crossConditionSpt) ||
            (LastPrice.Bid < spt.SuperTrend && crossConditionPivot))
            OpenPosition(TypeOperation.Sell,
                lastPivotPoint.PP.GetValueOrDefault(),
                lastPivotPoint.S3.GetValueOrDefault());
    }
}