using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests.Helpers;

public record GenerationTestResults(
    CSharpCompilation InputCompilation,
    ImmutableArray<Diagnostic> InputDiagnostics,
    ImmutableArray<SyntaxTree> InputSyntaxTrees,
    ImmutableDictionary<Type, GenerationTestResult> Results,
    CSharpCompilation FinalCompilation,
    ImmutableArray<Diagnostic> FinalDiagnostics,
    Assembly? Assembly
)
{
    public bool TryGetResult(Type type, [NotNullWhen(true)] out GenerationTestResult? result)
    {
        return Results.TryGetValue(type, out result);
    }

    public bool TryGetResult<T>([NotNullWhen(true)] out GenerationTestResult? result)
        where T : IIncrementalGenerator, new()
    {
        return Results.TryGetValue(typeof(T), out result);
    }

    public void EnsureDiagnosticSeverity(DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        Assert.Empty(InputDiagnostics.Where(x => x.Severity >= severity));
        foreach (var result in Results.Values)
        {
            result.EnsureDiagnosticSeverity(severity);
        }
    }

    public void AssertGeneratedAsExpected<T>(string expectedValue, params string[] expectedValues)
        where T : IIncrementalGenerator, new()
    {
        if (!TryGetResult<T>(out var result))
        {
            Assert.NotNull(result);
            return;
        }

        result.AssertGeneratedAsExpected(expectedValue);
    }

    public void AssertCompilationWasSuccessful()
    {
        Assert.Empty(
            InputDiagnostics
               .Where(z => !z.GetMessage().Contains("does not contain a definition for"))
               .Where(z => !z.GetMessage().Contains("Assuming assembly reference"))
               .Where(x => x.Severity >= DiagnosticSeverity.Warning)
        );
        foreach (var result in Results.Values)
        {
            result.EnsureDiagnosticSeverity();
        }
    }

    public void AssertGenerationWasSuccessful()
    {
        foreach (var item in Results.Values)
        {
            Assert.NotNull(item.Compilation);
            item.EnsureDiagnosticSeverity();
        }
    }

    public static implicit operator CSharpCompilation(GenerationTestResults results)
    {
        return results.FinalCompilation;
    }

    public static implicit operator Assembly?(GenerationTestResults results)
    {
        return results.Assembly;
    }
}
