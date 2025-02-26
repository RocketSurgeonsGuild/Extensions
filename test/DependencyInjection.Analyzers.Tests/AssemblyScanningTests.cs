
using Microsoft.CodeAnalysis;

using TestAssembly;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

[Timeout(ModuleInitializer.TestTimeout)]
public partial class AssemblyScanningTests : GeneratorTest
{
    [Test]
    [MethodDataSource(typeof(AssemblyScanningTestData), nameof(AssemblyScanningTestData.GetTestData))]
    public async Task Should_Generate_All_The_Things(TestSource item, CancellationToken cancellationToken)
    {
        var result = await Builder
                          .AddSources(item.Source)
                          .AddReferences(typeof(IService))
                          .AddCacheOptions(item.GetTempDirectory())
                          .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft")
                          .Build()
                          .GenerateAsync(cancellationToken);

        await Verify(result.AddCacheFiles())
             .AddScrubber(z => z.Replace(item.GetTempDirectory(), "{TempPath}"))
             .UseParameters(item.FileSafeName);
    }

    [Test]
    [MethodDataSource(typeof(AssemblyScanningTestData), nameof(AssemblyScanningTestData.GetTestData))]
    public async Task Should_Generate_All_The_Things_From_Another_Assembly(TestSource item, CancellationToken cancellationToken)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(item.Source)
                         .AddReferences(typeof(IService))
                         .AddCacheOptions(item.GetTempDirectory("other"))
                         .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection")
                         .Build()
                         .GenerateAsync();

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ShouldBeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddReferences(typeof(IService))
                          .AddCacheOptions(item.GetTempDirectory("test"))
                          .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection")
                          .Build()
                          .GenerateAsync(cancellationToken);

        await Verify(result.AddCacheFiles())
             .AddScrubber(z => z.Replace(item.GetTempDirectory(), "{TempPath}"))
             .UseParameters(item.FileSafeName);
    }

    [Test]
    [DependsOn(nameof(Should_Generate_All_The_Things_From_Another_Assembly), ProceedOnFailure = true)]
    [MethodDataSource(typeof(AssemblyScanningTestData), nameof(AssemblyScanningTestData.GetTestData))]
    public async Task Should_Generate_All_The_Things_From_Another_Assembly_Using_Cache(TestSource item, CancellationToken cancellationToken)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(item.Source)
                         .AddReferences(typeof(IService))
                         .PopulateCache(item.GetTempDirectory("other"))
                         .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection")
                         .Build()
                         .GenerateAsync(cancellationToken);

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ShouldBeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddReferences(typeof(IService))
                          .PopulateCache(item.GetTempDirectory("test"))
                          .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection")
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles())
             .AddScrubber(z => z.Replace(item.GetTempDirectory(), "{TempPath}"))
             .UseParameters(item.FileSafeName);
    }

    [Test]
    [Skip("This test is flaky")]
    [MethodDataSource(typeof(AssemblyScanningTestData), nameof(AssemblyScanningTestData.GetTestData))]
    public async Task Should_Generate_All_The_Things_From_Self_And_Another_Assembly(TestSource item, CancellationToken cancellationToken)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(item.Source)
                         .AddReferences(typeof(IService))
                         .AddCacheOptions(item.GetTempDirectory("other"))
                         .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection.Abstractions")
                         .Build()
                         .GenerateAsync(cancellationToken);

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ShouldBeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddSources(item.Source)
                          .AddReferences(typeof(IService))
                          .AddCacheOptions(item.GetTempDirectory("test"))
                          .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection.Abstractions")
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles())
             .AddScrubber(z => z.Replace(item.GetTempDirectory(), "{TempPath}"))
             .UseParameters(item.FileSafeName);
    }

    [Test]
    [Skip("This test is flaky")]
    [DependsOn(nameof(Should_Generate_All_The_Things_From_Self_And_Another_Assembly), ProceedOnFailure = true)]
    [MethodDataSource(typeof(AssemblyScanningTestData), nameof(AssemblyScanningTestData.GetTestData))]
    public async Task Should_Generate_All_The_Things_From_Self_And_Another_Assembly_Using_Cache(TestSource item, CancellationToken cancellationToken)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(item.Source)
                         .AddReferences(typeof(IService))
                         .PopulateCache(item.GetTempDirectory("other"))
                         .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection.Abstractions")
                         .Build()
                         .GenerateAsync(cancellationToken);

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ShouldBeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddSources(item.Source)
                          .AddReferences(typeof(IService))
                          .PopulateCache(item.GetTempDirectory("test"))
                          .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft.Extensions.DependencyInjection.Abstractions")
                          .Build()
                          .GenerateAsync(cancellationToken);

        await Verify(result.AddCacheFiles())
             .AddScrubber(z => z.Replace(item.GetTempDirectory(), "{TempPath}"))
             .UseParameters(item.FileSafeName);
    }

    [Test]
    [DependsOn(nameof(Should_Generate_All_The_Things), ProceedOnFailure = true)]
    [MethodDataSource(typeof(AssemblyScanningTestData), nameof(AssemblyScanningTestData.GetTestData))]
    public async Task Should_Generate_All_The_Things_Using_Cache(TestSource item, CancellationToken cancellationToken)
    {
        var result = await Builder
                          .AddSources(item.Source)
                          .AddReferences(typeof(IService))
                          .PopulateCache(item.GetTempDirectory())
                          .AddGlobalOption("build_property.ExcludeAssemblyFromCTP", "Microsoft")
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles())
             .AddScrubber(z => z.Replace(item.GetTempDirectory(), "{TempPath}"))
             .UseParameters(item.FileSafeName);
    }
}
