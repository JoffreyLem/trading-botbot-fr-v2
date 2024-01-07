using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Indicators.Attributes;
using RobotAppLibraryV2.Indicators.Indicator;

namespace RobotAppLibraryV2.Tests.Strategy.ImplementationTests.Indicator;

public class FakeStrategyContextIndicator : StrategyImplementationBase
{
    public SarIndicator SarIndicator { get; set; } = new();

    [IndicatorLongerTerm] public SarIndicator SarIndicator2 { get; set; } = new();

    public override string? Version => "1";

    public override void Run()
    {
    }
}