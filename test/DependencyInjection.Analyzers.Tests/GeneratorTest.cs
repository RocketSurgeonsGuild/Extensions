using System.Runtime.Loader;

using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public abstract partial class GeneratorTest() : LoggerTest(Defaults.LoggerTest)
{
    [Before(Test)]
    public void InitializeAsync()
    {
        _ = Directory.CreateDirectory(TempPath);
        Builder = GeneratorBuilderConstants
                 .Builder
                 .WithAssemblyLoadContext(AssemblyLoadContext);
        if (AssemblyLoadContext is not IDisposable disposable) return;

        Disposables.Add(disposable);
    }

    protected static void ClearCache(string tempPath)
    {
        if (!Directory.Exists(tempPath)) return;

        Directory.Delete(tempPath, true);
    }

    protected string GetTempPath()
    {
        var path = Path.Combine(TempPath, Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(path);
        return path;
    }

    protected AssemblyLoadContext AssemblyLoadContext { get; } = new CollectibleTestAssemblyLoadContext();
    protected GeneratorTestContextBuilder Builder { get; private set; } = null!;

    protected string TempPath { get; } = Path.Combine(ModuleInitializer.TempDirectory, Guid.NewGuid().ToString());
}
