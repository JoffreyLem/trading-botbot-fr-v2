using System.Reflection;
using System.Runtime;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyModel;
using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Indicators;
using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.StrategyDynamiqCompiler;

public static class StrategyDynamiqCompiler
{
    public static string ConvertByteToString(byte[] code)
    {
        return Encoding.UTF8.GetString(code);
    }

    public static bool TryCompileSourceCode(string sourceCode, out EmitResult compileResult,
        out byte[] compiledAssemblyBytes, out IEnumerable<Diagnostic> compileErrors)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(GCSettings).Assembly.Location)
        };

        foreach (var assembly in DependencyContext.Default.CompileLibraries
                     .SelectMany(cl => cl.ResolveReferencePaths()))
            references.Add(MetadataReference.CreateFromFile(assembly));


        references.Add(MetadataReference.CreateFromFile(typeof(StrategyImplementationBase).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(BaseIndicator<ResultBase>).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Candle).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(ResultBase).Assembly.Location));

        var dependencyContext = DependencyContext.Default;
        var runtimeAssemblies = dependencyContext.RuntimeLibraries
            .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
            .Select(Assembly.Load)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

        var compilation = CSharpCompilation.Create("StrategyDynamicAssembly")
            .AddReferences(references)
            .AddReferences(runtimeAssemblies)
            .AddSyntaxTrees(syntaxTree)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        compileResult = compilation.Emit(ms);
        compileErrors = compileResult.Diagnostics;
        compiledAssemblyBytes = compileResult.Success ? ms.ToArray() : null;

        return compileResult.Success;
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