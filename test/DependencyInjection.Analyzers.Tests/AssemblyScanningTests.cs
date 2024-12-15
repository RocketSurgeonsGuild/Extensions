using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public class AssemblyScanningTests : GeneratorTest
{
    [Test]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes(GetTypesTestsData.GetTypesItem getTypesItem)
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
                          .AddOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }

    [Test]
    [MethodDataSource(typeof(GetTypesTestsData), nameof(GetTypesTestsData.GetTypesData))]
    public async Task Should_Generate_Assembly_Provider_For_GetTypes_From_Another_Assembly(GetTypesTestsData.GetTypesItem getTypesItem)
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
                         .AddOptions(GetTempPath())
                         .Build()
                         .GenerateAsync();

        var diags = other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ToArray();
        if (diags.Length > 0) await Verify(diags).UseParameters(getTypesItem.Name).HashParameters()
                                                 .AddCacheFiles(TempPath);

        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .AddOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters()
                            .AddCacheFiles(TempPath);
    }
}
