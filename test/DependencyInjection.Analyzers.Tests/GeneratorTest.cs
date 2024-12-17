using System.ComponentModel;
using System.Runtime.Loader;
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

public abstract class GeneratorTest() : LoggerTest(Defaults.LoggerTest)
{
    protected string TempPath { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
}

internal static class VerifyExtensions
{
    public static SettingsTask AddCacheFiles(this SettingsTask task, string tempDirectory)
    {
        task.ScrubInlineGuids();
        //        Directory.EnumerateFiles(tempDirectory, "*", SearchOption.AllDirectories)
        //                 .ForEach(z =>
        //                          {
        //                              var content = File.ReadAllText(z);
        //                              if (content.Length == 0) return;
        //                              task.AppendFile(z);
        //                          }
        //                  );
        return task;
    }

    public static GeneratorTestContextBuilder AddOptions(this GeneratorTestContextBuilder builder, string tempPath) => builder
              .AddGlobalOption("build_property.BaseIntermediateOutputPath", "obj")
              .AddGlobalOption("build_property.ProjectDir", tempPath.Replace("\\", "/"));
}
