using System.ComponentModel;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

internal static class GeneratorBuilderConstants
{
    public static GeneratorTestContextBuilder Builder { get; } = GeneratorTestContextBuilder
                                                                .Create()
                                                                .WithGenerator<CompiledTypeProviderGenerator>()
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
    protected static void ClearCache(string tempPath)
    {
        if (!Directory.Exists(tempPath))
        {
            return;
        }

        Directory.Delete(tempPath, true);
    }

    protected string TempPath { get; } = Path.Combine(ModuleInitializer.TempDirectory, Guid.NewGuid().ToString());
    protected GeneratorTestContextBuilder Builder { get; private set; } = null!;
    protected AssemblyLoadContext AssemblyLoadContext { get; } = new CollectibleTestAssemblyLoadContext();

    [Before(Test)]
    public void InitializeAsync()
    {
        _ = Directory.CreateDirectory(TempPath);
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
        _ = Directory.CreateDirectory(path);
        return path;
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
        if (!Path.IsPathRooted(tempPath))
        {
            tempPath = Path.Combine(ModuleInitializer.TempDirectory, tempPath);
        }

        _ = Directory.CreateDirectory(tempPath);
        return builder
              .AddGlobalOption("build_property.IntermediateOutputPath", IntermediateOutputPath)
              .AddGlobalOption("build_property.ProjectDir", tempPath.Replace("\\", "/"));
    }

    public static GeneratorTestContextBuilder PopulateCache(this GeneratorTestContextBuilder builder, string tempPath)
    {
        if (!Path.IsPathRooted(tempPath))
        {
            tempPath = Path.Combine(ModuleInitializer.TempDirectory, tempPath);
        }

        var cachePath = $"{IntermediateOutputPath}/ctp/{Constants.CompiledTypeProviderCacheFileName}";
        var fullCachePath = Path.Combine(tempPath, cachePath);
        return ( !File.Exists(fullCachePath) )
            ? throw new FileNotFoundException("Cache file not found", fullCachePath)
            : builder
             .AddCacheOptions(tempPath)
             .AddAdditionalTexts(new GeneratorAdditionalText(cachePath.Replace("\\", "/"), SourceText.From(File.ReadAllText(fullCachePath))));
    }

    private const string IntermediateOutputPath = "obj/net9.0";

    internal class GeneratorAdditionalText(string path, SourceText sourceText) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText? GetText(CancellationToken cancellationToken = new()) => sourceText;
    }
}
