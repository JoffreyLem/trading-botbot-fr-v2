using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class PivotPoint : BaseIndicator<PivotPointsResult>
{
    public PivotPoint(PeriodSize periodSize, PivotPointType type = PivotPointType.Standard)
    {
        PeriodSize = periodSize;
        PivotPointType = type;
    }

    public PeriodSize PeriodSize { get; set; }
    public PivotPointType PivotPointType { get; set; }

    protected override IEnumerable<PivotPointsResult> Update(IEnumerable<Candle> data)
    {
        return data.GetPivotPoints(PeriodSize, PivotPointType);
    }
}