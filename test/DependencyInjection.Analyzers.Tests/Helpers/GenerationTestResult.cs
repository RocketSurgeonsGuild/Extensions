using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Sdk;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests.Helpers;

public record GenerationTestResult(
    CSharpCompilation Compilation,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<SyntaxTree> SyntaxTrees,
    ILogger Logger
)
{
    public void EnsureDiagnosticSeverity(DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        Diagnostics.Where(x => x.Severity >= severity).Should().HaveCount(0);
    }

    public void AssertGeneratedAsExpected(string expectedValue, params string[] expectedValues)
    {
        // normalize line endings to just LF
        var generatedText = SyntaxTrees.Select(z => NormalizeToLf(z.GetText().ToString()).Trim()).ToArray();
        // and append preamble to the expected
        var expectedText = new[] { expectedValue }.Concat(expectedValues).Select(z => NormalizeToLf(z).Trim()).ToArray();

        generatedText.Should().HaveCount(expectedText.Length);
        foreach (var (generated, expectedTxt) in generatedText.Zip(expectedText, (generated, expected) => ( generated, expected )))
        {
            try
            {
                generated.Should().Be(expectedTxt);
            }
            catch
            {
                try
                {
                    Assert.Equal(generated, expectedTxt);
                }
                catch (EqualException e2)
                {
                    Logger.LogCritical(e2.Message);
                }
                catch
                {
                    // ignore
                }

                throw;
            }
        }
    }

    public static string NormalizeToLf(string input)
    {
        return input.Replace(GenerationHelpers.CrLf, GenerationHelpers.Lf);
    }
}
