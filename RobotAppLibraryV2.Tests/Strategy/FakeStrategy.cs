using RobotAppLibraryV2.Strategy;

namespace RobotAppLibraryV2.Tests.Factory;

public class FakeStrategy : StrategyImplementationBase
{
    protected override void Run()
    {
        throw new NotImplementedException();
    }
}