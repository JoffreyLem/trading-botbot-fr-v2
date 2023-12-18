using RobotAppLibraryV2.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;

namespace StrategyApi.Strategy.StrategySar;

[VersionStrategy("pre-1")]
public class StrategySar : StrategyImplementationBase
{
    public bool CanOpen;

    public StrategySar()
    {
        DefaultStopLoss = 10;
        RunOnTick = false;
        UpdateOnTick = false;
        CloseOnTick = true;
    }

    public HeikiAshiIndicator HeikiAshi { get; set; } = new();

    public BollingerBand BollingerBand { get; set; } = new();

    public SarIndicator? SarIndicator { get; set; } = new(0.08);

    public SarIndicator? SarIndicator2 { get; set; } = new();

    public SuperTrend SuperTrend { get; set; } = new();

    public ForceIndex ForceIndex { get; set; } = new();


    protected override async void Run()
    {
        Logger.Information("Can Open : {canOpen}", CanOpen);
        var beforesar = SarIndicator?[^2];
        var sar = SarIndicator?.Last();
        var heiki = HeikiAshi?[^2];

        var c = beforesar is not null && sar is not null && heiki is not null;


        if (c)
        {
            if (beforesar.IsReversal.GetValueOrDefault()) CanOpen = true;

            if (heiki.IsStrongBuy())
            {
                if (sar.IsReversal == true || sar.Sar < (double?)LastPrice.Bid)
                    if (CanOpen)
                    {
                        CanOpen = false;
                        var tp = CalculateTakeProfit(10, TypeOperation.Buy);
                        OpenPositionAsync(TypeOperation.Buy, (decimal)sar.Sar, tp);
                    }
            }
            else if (heiki.IsStrongSell())
            {
                if (sar.IsReversal == true || sar.Sar > (double?)LastPrice.Ask)
                    if (CanOpen)
                    {
                        CanOpen = false;
                        var tp = CalculateTakeProfit(10, TypeOperation.Buy);
                        OpenPositionAsync(TypeOperation.Sell, (decimal)sar.Sar, tp);
                    }
            }
            else
            {
                Logger.Information("impossible d'ouvrir une position, conditions pas réunis");
            }
        }
        else
        {
            Logger.Warning("Indicator null");
        }
    }


    protected override bool ShouldUpdatePosition(Position position)
    {
        var sar = SarIndicator?.Last();
        if (position.TypePosition == TypeOperation.Buy)
        {
            if (sar?.Sar < (double?)LastPrice.Bid) position.StopLoss = (decimal?)sar.Sar;

            return true;
        }

        if (position.TypePosition == TypeOperation.Sell)
        {
            if (sar?.Sar > (double?)LastPrice.Ask) position.StopLoss = (decimal?)sar.Sar;

            return true;
        }

        return false;
    }


    protected override bool ShouldClosePosition(Position position)
    {
        var sar = SarIndicator.Last();
        var heiki = HeikiAshi?[^2];
        var isPositionBuy = position.TypePosition == TypeOperation.Buy;
        var isPositionSell = position.TypePosition == TypeOperation.Sell;

        var c1 = sar.IsReversal.GetValueOrDefault();

        var c2 = isPositionBuy && heiki.IsStrongBuy() == false;
        var c3 = isPositionSell && heiki.IsStrongSell() == false;

        if (c1 || c2 || c3) return true;

        return false;
    }
}