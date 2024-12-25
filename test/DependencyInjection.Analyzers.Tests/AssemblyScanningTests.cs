using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class AssemblyScanningTests : GeneratorTest
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    [Test]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes(GetTypesTestsData.GetTypesItem getTypesItem)
    {
        ClearCache(GetTestCache());
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
                               	        provider.GetTypes({{{getTypesItem.Expression}}});
                                   }
                               }
                               """"
                           )
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes_From_Another_Assembly(GetTypesTestsData.GetTypesItem getTypesItem)
    {
        ClearCache(GetTestCache());
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
                              	       provider.GetTypes({{{getTypesItem.Expression}}});
                                  }
                              }
                              """"
                          )
                         .AddOptions(GetTestCache())
                         .Build()
                         .GenerateAsync();

        var diags = other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ToArray();
        if (diags.Length > 0)
        {
            await Verify(diags).UseParameters(getTypesItem.Name).HashParameters()
                                                 .AddCacheFiles(TempPath);
        }

        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies(GetAssembliesTestsData.GetAssembliesItem getTypesItem)
    {
        ClearCache(GetTestCache());
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
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies_From_Another_Assembly(GetAssembliesTestsData.GetAssembliesItem getTypesItem)
    {
        ClearCache(GetTestCache());
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
                         .AddOptions(GetTestCache())
                         .Build()
                         .GenerateAsync();

        var diags = other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ToArray();
        if (diags.Length > 0)
        {
            await Verify(diags).UseParameters(getTypesItem.Name).HashParameters()
                                                 .AddCacheFiles(TempPath);
        }

        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetTypes))]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes_Using_Cache(GetTypesTestsData.GetTypesItem getTypesItem)
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
                               	        provider.GetTypes({{{getTypesItem.Expression}}});
                                   }
                               }
                               """"
                           )
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetTypes_From_Another_Assembly))]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes_From_Another_Assembly_Using_Cache(GetTypesTestsData.GetTypesItem getTypesItem)
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
                              	       provider.GetTypes({{{getTypesItem.Expression}}});
                                  }
                              }
                              """"
                          )
                         .AddOptions(GetTestCache())
                         .Build()
                         .GenerateAsync();

        var diags = other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ToArray();
        if (diags.Length > 0)
        {
            await Verify(diags).UseParameters(getTypesItem.Name).HashParameters()
                                                 .AddCacheFiles(TempPath);
        }

        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetAssemblies))]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies_Using_Cache(GetAssembliesTestsData.GetAssembliesItem getTypesItem)
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
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [DependsOn(nameof(Should_Generate_Assembly_Provider_For_GetAssemblies_From_Another_Assembly))]
    [MethodDataSource(typeof(GetAssembliesTestsData), nameof(GetAssembliesTestsData.GetAssembliesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetAssemblies_From_Another_Assembly_Using_Cache(GetAssembliesTestsData.GetAssembliesItem getTypesItem)
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
                         .AddOptions(GetTestCache())
                         .Build()
                         .GenerateAsync();

        var diags = other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ToArray();
        if (diags.Length > 0)
        {
            await Verify(diags).UseParameters(getTypesItem.Name).HashParameters()
                                                 .AddCacheFiles(TempPath);
        }

        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddOptions(GetTestCache())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }
}
