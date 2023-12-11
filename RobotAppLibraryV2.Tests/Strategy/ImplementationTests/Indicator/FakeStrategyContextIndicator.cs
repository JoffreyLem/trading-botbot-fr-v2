using RobotAppLibraryV2.Attributes;
using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;
using RobotAppLibraryV2.Strategy;

namespace RobotAppLibraryV2.Tests.Strategy.ImplementationTests.Indicator;

[VersionStrategy("1")]
public class FakeStrategyContextIndicator : StrategyImplementationBase
{
    public SarIndicator SarIndicator { get; set; } = new();

    [IndicatorLongerTerm] public SarIndicator SarIndicator2 { get; set; } = new();

    protected internal override void Run()
    {
    }
}