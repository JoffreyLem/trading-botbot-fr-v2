using System.Reflection;
using System.Runtime.Loader;

namespace RobotAppLibraryV2.StrategyDynamiqCompiler;

public class CustomLoadContext : AssemblyLoadContext
{
    public CustomLoadContext() : base(true)
    {
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }
}