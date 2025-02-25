using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

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

        _ = Directory.CreateDirectory(tempPath);
        return builder
              .AddGlobalOption("build_property.IntermediateOutputPath", IntermediateOutputPath)
              .AddGlobalOption("build_property.ProjectDir", tempPath.Replace("\\", "/"));
    }

    public static GeneratorTestContextBuilder PopulateCache(this GeneratorTestContextBuilder builder, string tempPath)
    {
        if (!Path.IsPathRooted(tempPath)) tempPath = Path.Combine(ModuleInitializer.TempDirectory, tempPath);

        var cachePath = $"{IntermediateOutputPath}/ctp/{Constants.CompiledTypeProviderCacheFileName}";
        var fullCachePath = Path.Combine(tempPath, cachePath);
        return !File.Exists(fullCachePath)
            ? throw new FileNotFoundException("Cache file not found", fullCachePath)
            : builder
             .AddCacheOptions(tempPath)
             .AddAdditionalTexts(new GeneratorAdditionalText(cachePath.Replace("\\", "/"), SourceText.From(File.ReadAllText(fullCachePath))));
    }

    internal class GeneratorAdditionalText(string path, SourceText sourceText) : AdditionalText
    {
        public override SourceText? GetText(CancellationToken cancellationToken = new()) => sourceText;
        public override string Path { get; } = path;
    }

    private const string IntermediateOutputPath = "obj/net9.0";
}