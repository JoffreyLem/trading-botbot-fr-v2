using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyModel;

namespace RobotAppLibraryV2.StrategyDynamiqCompiler;

public static class StrategyDynamiqCompiler
{
    public static byte[] TryCompileSourceCode(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // var references = new List<MetadataReference>
        // {
        //     MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        //     MetadataReference.CreateFromFile(typeof(GCSettings).Assembly.Location),
        //     MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location) 
        //
        // };
        //
        // foreach (var assembly in DependencyContext.Default.CompileLibraries
        //              .SelectMany(cl => cl.ResolveReferencePaths()))
        //     references.Add(MetadataReference.CreateFromFile(assembly));
        //
        //
        // references.Add(MetadataReference.CreateFromFile(typeof(StrategyImplementationBase).Assembly.Location));
        // references.Add(MetadataReference.CreateFromFile(typeof(BaseIndicator<ResultBase>).Assembly.Location));
        // references.Add(MetadataReference.CreateFromFile(typeof(Candle).Assembly.Location));
        // references.Add(MetadataReference.CreateFromFile(typeof(ResultBase).Assembly.Location));

        var dependencyContext = DependencyContext.Default;
        var runtimeAssemblies = dependencyContext.RuntimeLibraries
            .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
            .Select(Assembly.Load)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithOverflowChecks(false);

        var compilation = CSharpCompilation.Create("StrategyDynamicAssembly")
            .AddReferences(runtimeAssemblies)
            .AddSyntaxTrees(syntaxTree)
            .WithOptions(compilationOptions);

        using var ms = new MemoryStream();
        var compileResult = compilation.Emit(ms);

        if (compileResult.Success) return ms.ToArray();
        IEnumerable<Diagnostic> compileErrors = compileResult.Diagnostics;
        throw new CompilationException("La compilation a échoué.",
            compileErrors.Where(error => error.Severity == DiagnosticSeverity.Error));
    }

    public static string? GetFirstClassName(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = syntaxTree.GetRoot() as CompilationUnitSyntax;

        var firstClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (firstClass == null) return null;

        var namespaceDeclaration = firstClass.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDeclaration != null
            ? $"{namespaceDeclaration.Name}.{firstClass.Identifier.ValueText}"
            : firstClass.Identifier.ValueText;
    }
}