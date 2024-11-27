using System.ComponentModel;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public abstract class GeneratorTest() : LoggerTest(Defaults.LoggerTest)
{
    protected GeneratorTestContextBuilder Builder { get; private set; } = null!;
    protected AssemblyLoadContext AssemblyLoadContext { get; } = new CollectibleTestAssemblyLoadContext();

    public virtual Task InitializeAsync()
    {
        Builder = GeneratorTestContextBuilder
                 .Create()
                 .WithGenerator<CompiledServiceScanningGenerator>()
                 .WithAssemblyLoadContext(AssemblyLoadContext)
                 .AddReferences(
                      typeof(ActivatorUtilities).Assembly,
                      typeof(IServiceProvider).Assembly,
                      typeof(IServiceCollection).Assembly,
                      typeof(ServiceCollection).Assembly,
                      typeof(ServiceRegistrationAttribute).Assembly,
                      typeof(EditorBrowsableAttribute).Assembly,
                      typeof(Attribute).Assembly
                  )
                 .IgnoreOutputFile("CompiledServiceScanningExtensions.cs");
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        if (AssemblyLoadContext is IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        Disposables.Dispose();
        return Task.CompletedTask;
    }
}
