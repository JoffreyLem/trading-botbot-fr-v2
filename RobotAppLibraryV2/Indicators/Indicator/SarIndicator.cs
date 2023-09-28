using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Indicator;

public class SarIndicator : BaseIndicator<ParabolicSarResult>
{
    public SarIndicator(double accelerationStep = 0.02, double maxAccelerationFactor = 0.2)
    {
        AccelerationStep = accelerationStep;
        MaxAccelerationFactor = maxAccelerationFactor;
    }

    public double AccelerationStep { get; init; }

    public double MaxAccelerationFactor { get; init; }

    protected override List<ParabolicSarResult> Update(List<Candle> data)
    {
        return data.GetParabolicSar(AccelerationStep, MaxAccelerationFactor).ToList();
    }

    public bool IsBuy()
    {
        return this.Last().Sar < (double?)LastTick.Bid;
    }

    public bool IsSell()
    {
        return this.Last().Sar > (double?)LastTick.Bid;
    }
}