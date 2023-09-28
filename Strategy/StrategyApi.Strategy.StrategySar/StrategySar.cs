using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;
using Serilog;
using Skender.Stock.Indicators;


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

    public HeikiAshiIndicator HeikiAshi { get; set; } = new HeikiAshiIndicator();

    public BollingerBand BollingerBand { get; set; } = new BollingerBand();

    public SarIndicator? SarIndicator { get; set; } = new SarIndicator(0.08);

    public SarIndicator? SarIndicator2 { get; set; } = new SarIndicator();

    public SuperTrend SuperTrend { get; set; } = new SuperTrend();

    public ForceIndex ForceIndex { get; set; } = new ForceIndex();


    protected override async void Run()
    {
        Logger.Information("Can Open : {canOpen}", CanOpen);
        ParabolicSarResult? beforesar = SarIndicator?[^2];
        ParabolicSarResult? sar = SarIndicator?.Last();
        HeikinAshiResult? heiki = HeikiAshi?[^2];

        bool c = beforesar is not null && sar is not null && heiki is not null;


        if (c)
        {
            if (beforesar.IsReversal.GetValueOrDefault())
            {
                CanOpen = true;
            }

            if (heiki.IsStrongBuy())
            {
                if (sar.IsReversal == true || sar.Sar < (double?)LastPrice.Bid)
                {
                    if (CanOpen)
                    {
                        CanOpen = false;
                        decimal tp = CalculateTakeProfit(10, TypePosition.Buy);
                        OpenPosition(TypePosition.Buy, (decimal)sar.Sar, tp);
                    }
                }
            }
            else if (heiki.IsStrongSell())
            {
                if (sar.IsReversal == true || sar.Sar > (double?)LastPrice.Ask)
                {
                    if (CanOpen)
                    {
                        CanOpen = false;
                        decimal tp = CalculateTakeProfit(10, TypePosition.Buy);
                        OpenPosition(TypePosition.Sell, (decimal)sar.Sar, tp);
                    }
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
        ParabolicSarResult? sar = SarIndicator?.Last();
        if (position.TypePosition == TypePosition.Buy)
        {
            if (sar?.Sar < (double?)LastPrice.Bid)
            {
                position.StopLoss = (decimal?)sar.Sar;
            }

            return true;
        }

        if (position.TypePosition == TypePosition.Sell)
        {
            if (sar?.Sar > (double?)LastPrice.Ask)
            {
                position.StopLoss = (decimal?)sar.Sar;
            }

            return true;
        }

        return false;
    }


    protected override bool ShouldClosePosition(Position position)
    {
        ParabolicSarResult sar = SarIndicator.Last();
        HeikinAshiResult? heiki = HeikiAshi?[^2];
        bool isPositionBuy = position.TypePosition == TypePosition.Buy;
        bool isPositionSell = position.TypePosition == TypePosition.Sell;

        bool c1 = sar.IsReversal.GetValueOrDefault();

        bool c2 = isPositionBuy && heiki.IsStrongBuy() == false;
        bool c3 = isPositionSell && heiki.IsStrongSell() == false;

        if (c1 || c2 || c3)
        {
            return true;
        }

        return false;
    }
}