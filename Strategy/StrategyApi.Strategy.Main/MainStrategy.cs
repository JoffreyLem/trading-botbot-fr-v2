using RobotAppLibraryV2.Attributes;
using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;
using Skender.Stock.Indicators;

namespace StrategyApi.Strategy.Main;

[VersionStrategy("prod-test-2")]
public class MainStrategy : StrategyImplementationBase
{
    public MainStrategy()
    {
        DefaultStopLoss = 75;
        RunOnTick = false;
        UpdateOnTick = false;
        CloseOnTick = false;
        CanRun = true;
    }

    public PivotPoint PivotPoint { get; set; } = new(PeriodSize.Day);

    public SarIndicator SarIndicator { get; set; } = new();

    public BollingerBand BollingerBand { get; set; } = new();

    public SuperTrend SuperTrend { get; set; } = new();


    [IndicatorLongerTerm] public HeikiAshiIndicator B_HeikiAshiIndicator { get; set; } = new();


    [IndicatorLongerTerm] public SarIndicator B_SarIndicator { get; set; } = new();


    protected override void Run()
    {
        var lastPivotPoint = PivotPoint.LastOrDefault();
        var tp = PivotPoint.LastOrDefault();
        var sl = SarIndicator.LastOrDefault()?.Sar;
        if (LastPrice.Bid < lastPivotPoint?.R1 && LastPrice.Bid > lastPivotPoint?.S1)
        {
            var sarSelected = SarIndicator[^2];
            var sarReversalTchech = sarSelected?.IsReversal;
            var superTrandSelected = SuperTrend.LastOrDefault();
            var lastSarB = B_SarIndicator.LastOrDefault();
            var lastHashi = B_HeikiAshiIndicator.LastOrDefault();

            if (sarReversalTchech is true)
            {
                if (LastPrice.Bid > superTrandSelected?.SuperTrend)
                    OpenBuy(lastSarB, lastHashi, superTrandSelected, sarSelected, sl, tp);
                else if (LastPrice.Bid < superTrandSelected?.SuperTrend)
                    OpenSell(lastSarB, lastHashi, superTrandSelected, sarSelected, sl, tp);
            }
            else
            {
                var candle3 = History[^3];
                var superTrendBuyCrossTcheck = candle3.Close < superTrandSelected?.SuperTrend &&
                                               LastCandle.Close > superTrandSelected?.SuperTrend;

                var superTrendSellCrossTcheck = candle3.Close > superTrandSelected?.SuperTrend &&
                                                LastCandle.Close < superTrandSelected?.SuperTrend;

                if (sarSelected?.Sar < (double?)LastCandle.Close && superTrendBuyCrossTcheck)
                    OpenBuy(lastSarB, lastHashi, superTrandSelected, sarSelected, sl, tp);
                else if (sarSelected?.Sar > (double?)LastCandle.Close && superTrendSellCrossTcheck)
                    OpenSell(lastSarB, lastHashi, superTrandSelected, sarSelected, sl, tp);
            }
        }
    }

    private void OpenBuy(ParabolicSarResult? lastSarB, HeikinAshiResult? lastHashi,
        SuperTrendResult? superTrandSelected, ParabolicSarResult? sarSelected, double? sl, PivotPointsResult? tp)
    {
        if (lastSarB?.Sar < (double)LastCandle.Close && (bool)lastHashi?.IsStrongBuy())
        {
            if (superTrandSelected?.SuperTrend > (decimal?)sarSelected?.Sar)
                sl = (double?)superTrandSelected.SuperTrend;

            OpenPosition(TypeOperation.Buy, (decimal)sl, (decimal)tp?.R3);
        }
    }

    private void OpenSell(ParabolicSarResult? lastSarB, HeikinAshiResult? lastHashi,
        SuperTrendResult? superTrandSelected, ParabolicSarResult? sarSelected, double? sl, PivotPointsResult? tp)
    {
        if (lastSarB?.Sar > (double)LastCandle.Close && (bool)lastHashi?.IsStrongSell())
        {
            if (superTrandSelected?.SuperTrend < (decimal?)sarSelected?.Sar)
                sl = (double?)superTrandSelected.SuperTrend;

            OpenPosition(TypeOperation.Sell, (decimal)sl, (decimal)tp?.S3);
        }
    }


    protected override bool ShouldUpdatePosition(Position position)
    {
        var lastPivot = PivotPoint.LastOrDefault();
        var lastSar = SarIndicator.LastOrDefault();

        if (position.TypePosition == TypeOperation.Buy)
        {
            position.StopLoss = CalculateBuyStopLoss(lastPivot, lastSar);
            return true;
        }

        if (position.TypePosition == TypeOperation.Sell)
        {
            position.StopLoss = CalculateSellStopLoss(lastPivot, lastSar);
            return true;
        }

        return false;
    }

    private decimal? CalculateBuyStopLoss(PivotPointsResult? lastPivot, ParabolicSarResult? lastSar)
    {
        if (LastCandle.Close > lastPivot?.R1)
            return lastSar?.Sar > (double)lastPivot?.R1 ? (decimal?)lastSar?.Sar : lastPivot?.R1;

        if (LastCandle.Close > lastPivot?.R2)
            return lastSar?.Sar > (double)lastPivot?.R2 ? (decimal?)lastSar?.Sar : lastPivot?.R2;

        return (decimal?)lastSar?.Sar;
    }

    private decimal? CalculateSellStopLoss(PivotPointsResult? lastPivot, ParabolicSarResult? lastSar)
    {
        if (LastCandle.Close < lastPivot?.S1)
            return lastSar?.Sar < (double)lastPivot?.S1 ? (decimal?)lastSar?.Sar : lastPivot?.S1;

        if (LastCandle.Close < lastPivot?.S2)
            return lastSar?.Sar < (double)lastPivot?.S2 ? (decimal?)lastSar?.Sar : lastPivot?.S2;

        return (decimal?)lastSar?.Sar;
    }


    protected override bool ShouldClosePosition(Position position)
    {
        var sarToCheck = SarIndicator[^2];
        if (sarToCheck.IsReversal is true) return true;

        if (position.TypePosition == TypeOperation.Buy)
        {
        }
        else if (position.TypePosition == TypeOperation.Sell)
        {
        }

        return false;
    }
}