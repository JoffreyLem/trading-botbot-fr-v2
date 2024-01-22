using Microsoft.CodeAnalysis;

namespace RobotAppLibraryV2.StrategyDynamiqCompiler;

public class CompilationException : Exception
{
    public CompilationException(string message, IEnumerable<Diagnostic> compileErrors)
        : base(message)
    {
        CompileErrors = compileErrors;
    }

    public IEnumerable<Diagnostic> CompileErrors { get; }
}