using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Scrutor;
using Xunit.Abstractions;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public abstract class GeneratorTest
    (ITestOutputHelper testOutputHelper, bool compiled_scan_assembly_load) : LoggerTest(testOutputHelper, LogLevel.Trace), IAsyncLifetime
{
    public GeneratorTestContextBuilder Builder { get; protected set; } = null!;
    public AssemblyLoadContext AssemblyLoadContext { get; } = new CollectibleTestAssemblyLoadContext();

    public virtual Task InitializeAsync()
    {
        Builder = GeneratorTestContextBuilder
                 .Create()
                 .WithGenerator<CompiledServiceScanningGenerator>()
                 .WithAssemblyLoadContext(AssemblyLoadContext)
                 .AddReferences(
                      typeof(ActivatorUtilities).Assembly,
                      typeof(IServiceProvider).Assembly,
                      typeof(IFluentInterface).Assembly,
                      typeof(IServiceCollection).Assembly,
                      typeof(ServiceCollection).Assembly,
                      typeof(ServiceRegistrationAttribute).Assembly
                  )
                 .IgnoreOutputFile("CompiledServiceScanningExtensions.cs")
                 .AddGlobalOption("compiled_scan_assembly_load", compiled_scan_assembly_load ? "true" : "false");
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