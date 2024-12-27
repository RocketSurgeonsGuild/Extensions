using System.ComponentModel;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

internal static class GeneratorBuilderConstants
{
    public static GeneratorTestContextBuilder Builder { get; } = GeneratorTestContextBuilder
                                                                .Create()
                                                                .WithGenerator<CompiledServiceScanningGenerator>()
                                                                .AddReferences(
                                                                     typeof(ActivatorUtilities).Assembly,
                                                                     typeof(IServiceProvider).Assembly,
                                                                     typeof(IServiceCollection).Assembly,
                                                                     typeof(ServiceCollection).Assembly,
                                                                     typeof(ServiceRegistrationAttribute).Assembly,
                                                                     typeof(EditorBrowsableAttribute).Assembly,
                                                                     typeof(Attribute).Assembly
                                                                 );
}

public abstract partial class GeneratorTest() : LoggerTest(Defaults.LoggerTest)
{
    protected string TempPath { get; } = Path.Combine(ModuleInitializer.TempDirectory, Guid.NewGuid().ToString());
    protected GeneratorTestContextBuilder Builder { get; private set; } = null!;
    protected AssemblyLoadContext AssemblyLoadContext { get; } = new CollectibleTestAssemblyLoadContext();

    [Before(Test)]
    public void InitializeAsync()
    {
        Directory.CreateDirectory(TempPath);
        Builder = GeneratorBuilderConstants
                 .Builder
                 .WithAssemblyLoadContext(AssemblyLoadContext);
        if (AssemblyLoadContext is not IDisposable disposable)
        {
            return;
        }

        Disposables.Add(disposable);
    }

    protected string GetTempPath()
    {
        var path = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    protected static void ClearCache(string tempPath)
    {
        if (Directory.Exists(tempPath))
            Directory.Delete(tempPath, true);
    }
}

internal static partial class VerifyExtensions
{
    public static GeneratorTestResultsWithCacheFiles AddCacheFiles(this GeneratorTestResults results)
    {
        var projectPath = results.GlobalOptions["build_property.ProjectDir"];
        var tempPath = Path.Combine(projectPath, results.GlobalOptions["build_property.IntermediateOutputPath"]);
        return new(results, tempPath);
    }

    public static GeneratorTestContextBuilder AddCacheOptions(this GeneratorTestContextBuilder builder, string tempPath)
    {
        if (!Path.IsPathRooted(tempPath)) tempPath = Path.Combine(ModuleInitializer.TempDirectory, tempPath);
        Directory.CreateDirectory(tempPath);
        return builder
              .AddGlobalOption("build_property.IntermediateOutputPath", "obj/net9.0")
              .AddGlobalOption("build_property.ProjectDir", tempPath.Replace("\\", "/"));
    }

    public static GeneratorTestContextBuilder PopulateCache(this GeneratorTestContextBuilder builder, string tempPath)
    {
        if (!Path.IsPathRooted(tempPath)) tempPath = Path.Combine(ModuleInitializer.TempDirectory, tempPath);
        var files = Directory
                   .EnumerateFiles(tempPath, "*", SearchOption.AllDirectories)
                   .Select(z => new GeneratorAdditionalText(z.Replace("\\", "/"), SourceText.From(File.ReadAllText(z))))
                   .OfType<AdditionalText>()
                   .ToArray();
        return builder
              .AddCacheOptions(tempPath)
              .AddAdditionalTexts(files);
    }

    internal class GeneratorAdditionalText(string path, SourceText sourceText) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText? GetText(CancellationToken cancellationToken = new())
        {
            return sourceText;
        }
    }
}
