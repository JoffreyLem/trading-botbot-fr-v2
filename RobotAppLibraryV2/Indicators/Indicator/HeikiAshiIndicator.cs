using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class HeikiAshiIndicator : BaseIndicator<HeikinAshiResult>
{
    public HeikiAshiIndicator(int loopBackPeriodRequested = 20)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }


    protected override List<HeikinAshiResult> Update(List<Candle> data)
    {
        return data.GetHeikinAshi().ToList();
    }
}

public static class HeikiAshiIndicatorHelper
{
    public static bool IsStrongBuy(this HeikinAshiResult heiki)
    {
        if (heiki.Low == heiki.Open && heiki.Close >= heiki.Open) return true;

        return false;
    }

    public static bool IsBuy(this HeikinAshiResult heiki)
    {
        if (heiki.Close >= heiki.Open) return true;

        return false;
    }

    public static bool IsStrongSell(this HeikinAshiResult heiki)
    {
        if (heiki.High == heiki.Open && heiki.Close <= heiki.Open) return true;

        return false;
    }

    public static bool IsSell(this HeikinAshiResult heiki)
    {
        if (heiki.Close <= heiki.Open) return true;

        return false;
    }
}