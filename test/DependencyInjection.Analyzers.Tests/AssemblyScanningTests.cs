using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;



[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class AssemblyScanningTests : GeneratorTest
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    [Test]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes(GetTypesTestsData.Item item)
    {
        var result = await Builder
                          .AddSources(
                               $$$""""
                               using Rocket.Surgery.DependencyInjection;
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;
                               using System.ComponentModel;
                               using System.Threading;
                               using System.Threading.Tasks;
                               using System;

                               public static class Program
                               {
                                   static void Main()
                                   {
                                        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	        provider.GetTypes({{{item.Expression}}});
                                   }
                               }
                               """"
                           )
                          .AddCacheOptions(item.GetTempDirectory())
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(item.Name).HashParameters();
    }

    [Test]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes_From_Another_Assembly(GetTypesTestsData.Item item)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(
                              $$$""""
                              using Rocket.Surgery.DependencyInjection;
                              using Rocket.Surgery.DependencyInjection.Compiled;
                              using Microsoft.Extensions.DependencyInjection;
                              using System.ComponentModel;
                              using System.Threading;
                              using System.Threading.Tasks;
                              using System;

                              public static class Program
                              {
                                  static void Main()
                                  {
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                              	       provider.GetTypes({{{item.Expression}}});
                                  }
                              }
                              """"
                          )
                         .AddCacheOptions(item.GetTempDirectory("other"))
                         .Build()
                         .GenerateAsync();

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).Should().BeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddCacheOptions(item.GetTempDirectory("test"))
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(item.Name).HashParameters();
    }

    [Test]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies(GetAssembliesTestsData.Item getTypesItem)
    {
        var result = await Builder
                          .AddSources(
                               $$$""""
                               using Rocket.Surgery.DependencyInjection;
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;
                               using System.ComponentModel;
                               using System.Threading;
                               using System.Threading.Tasks;
                               using System;

                               public static class Program
                               {
                                   static void Main()
                                   {
                                        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	        provider.GetAssemblies({{{getTypesItem.Expression}}});
                                   }
                               }
                               """"
                           )
                          .AddCacheOptions(getTypesItem.GetTempDirectory())
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(getTypesItem.Name).HashParameters();
    }

    [Test]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies_From_Another_Assembly(GetAssembliesTestsData.Item getTypesItem)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(
                              $$$""""
                              using Rocket.Surgery.DependencyInjection;
                              using Rocket.Surgery.DependencyInjection.Compiled;
                              using Microsoft.Extensions.DependencyInjection;
                              using System.ComponentModel;
                              using System.Threading;
                              using System.Threading.Tasks;
                              using System;

                              public static class Program
                              {
                                  static void Main()
                                  {
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                              	       provider.GetAssemblies({{{getTypesItem.Expression}}});
                                  }
                              }
                              """"
                          )
                         .AddCacheOptions(getTypesItem.GetTempDirectory("other"))
                         .Build()
                         .GenerateAsync();

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).Should().BeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddCacheOptions(getTypesItem.GetTempDirectory("test"))
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(getTypesItem.Name).HashParameters();
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetTypes), ProceedOnFailure = true)]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes_Using_Cache(GetTypesTestsData.Item item)
    {
        var result = await Builder
                          .AddSources(
                               $$$""""
                               using Rocket.Surgery.DependencyInjection;
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;
                               using System.ComponentModel;
                               using System.Threading;
                               using System.Threading.Tasks;
                               using System;

                               public static class Program
                               {
                                   static void Main()
                                   {
                                        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	        provider.GetTypes({{{item.Expression}}});
                                   }
                               }
                               """"
                           )
                          .PopulateCache(item.GetTempDirectory())
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(item.Name).HashParameters();
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetTypes_From_Another_Assembly), ProceedOnFailure = true)]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes_From_Another_Assembly_Using_Cache(GetTypesTestsData.Item item)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(
                              $$$""""
                              using Rocket.Surgery.DependencyInjection;
                              using Rocket.Surgery.DependencyInjection.Compiled;
                              using Microsoft.Extensions.DependencyInjection;
                              using System.ComponentModel;
                              using System.Threading;
                              using System.Threading.Tasks;
                              using System;

                              public static class Program
                              {
                                  static void Main()
                                  {
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                              	       provider.GetTypes({{{item.Expression}}});
                                  }
                              }
                              """"
                          )
                         .PopulateCache(item.GetTempDirectory("other"))
                         .Build()
                         .GenerateAsync();

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).Should().BeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .PopulateCache(item.GetTempDirectory("test"))
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(item.Name).HashParameters();
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetAssemblies), ProceedOnFailure = true)]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies_Using_Cache(GetAssembliesTestsData.Item getTypesItem)
    {
        var result = await Builder
                          .AddSources(
                               $$$""""
                               using Rocket.Surgery.DependencyInjection;
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;
                               using System.ComponentModel;
                               using System.Threading;
                               using System.Threading.Tasks;
                               using System;

                               public static class Program
                               {
                                   static void Main()
                                   {
                                        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	        provider.GetAssemblies({{{getTypesItem.Expression}}});
                                   }
                               }
                               """"
                           )
                          .PopulateCache(getTypesItem.GetTempDirectory())
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(getTypesItem.Name).HashParameters();
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetAssemblies_From_Another_Assembly), ProceedOnFailure = true)]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies_From_Another_Assembly_Using_Cache(GetAssembliesTestsData.Item getTypesItem)
    {
        using var assemblyLoadContext = new CollectibleTestAssemblyLoadContext();
        var other = await Builder
                         .WithProjectName("OtherProject")
                         .AddSources(
                              $$$""""
                              using Rocket.Surgery.DependencyInjection;
                              using Rocket.Surgery.DependencyInjection.Compiled;
                              using Microsoft.Extensions.DependencyInjection;
                              using System.ComponentModel;
                              using System.Threading;
                              using System.Threading.Tasks;
                              using System;

                              public static class Program
                              {
                                  static void Main()
                                  {
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                              	       provider.GetAssemblies({{{getTypesItem.Expression}}});
                                  }
                              }
                              """"
                          )
                         .PopulateCache(getTypesItem.GetTempDirectory("other"))
                         .Build()
                         .GenerateAsync();

        other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).Should().BeEmpty();
        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .PopulateCache(getTypesItem.GetTempDirectory("test"))
                          .Build()
                          .GenerateAsync();

        await Verify(result.AddCacheFiles()).UseParameters(getTypesItem.Name).HashParameters();
    }
}
