using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;

namespace RobotAppLibraryV2.Tests.Strategy.ImplementationTests.Volume;

public class FakeStrategyTestContextNoSlVolume : StrategyImplementationBase
{
    public SarIndicator SarIndicator { get; set; } = new();

    [IndicatorLongerTerm] public SarIndicator SarIndicator2 { get; set; } = new();

    protected override void Run()
    {
        OpenPosition(TypePosition.Buy, 0, 1, 0, 0.10);
    }
}