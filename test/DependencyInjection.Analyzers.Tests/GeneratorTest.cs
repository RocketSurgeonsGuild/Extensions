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

internal abstract class GeneratorTest() : LoggerTest(Defaults.LoggerTest)
{
    protected GeneratorTestContextBuilder Builder { get; private set; } = null!;
    protected AssemblyLoadContext AssemblyLoadContext { get; } = new CollectibleTestAssemblyLoadContext();

    [Before(Test)]
    public void InitializeAsync()
    {
        Builder = GeneratorBuilderConstants.Builder.WithAssemblyLoadContext(AssemblyLoadContext);
        if (AssemblyLoadContext is not IDisposable disposable)
        {
            return;
        }

        Disposables.Add(disposable);
    }
}
