using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Xunit.Abstractions;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public class AssemblyScanningTests(ITestOutputHelper testOutputHelper) : GeneratorTest(testOutputHelper, false)
{
    [Theory]
    [MemberData(nameof(GetTypesTestsData.GetTypesData), MemberType = typeof(GetTypesTestsData))]
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
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters();
    }


    [Theory]
    [MemberData(nameof(GetTypesTestsData.GetTypesData), MemberType = typeof(GetTypesTestsData))]
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
                         .Build()
                         .GenerateAsync();

        var diags = other.FinalDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error).ToArray();
        if (diags.Length > 0) await Verify(diags).UseParameters(getTypesItem.Name).HashParameters();

        other.EnsureDiagnosticSeverity(DiagnosticSeverity.Error);

        var result = await Builder
                          .AddCompilationReferences(other)
                          .Build()
                          .GenerateAsync();

        await Verify(result).UseParameters(getTypesItem.Name).HashParameters();
    }
}

public class StaticScanningTests(ITestOutputHelper testOutputHelper) : GeneratorTest(testOutputHelper, false)
{
    [Fact]
    public async Task Should_Handle_Public_Types()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService
{

}

public class Service : IService
{

}

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableTo(typeof(IService)))
                .AsSelf()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Handle_Private_Types()
    {
        var dependencies = new List<GeneratorTestResults>();

        var rootGenerator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("RootDependencyProject")
                                 .WithAssemblyLoadContext(AssemblyLoadContext)
                                 .AddSources(
                                      @"
namespace RootDependencyProject
{
    public interface IService { }
    class Service : IService { }
}
"
                                  )
                                 .Build()
                                 .GenerateAsync();
        dependencies.Add(rootGenerator);

        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;
using RootDependencyProject;

namespace TestProject
{
    public static class Program
    {
        static void Main() { }
        static IServiceCollection LoadServices()
        {
            var services = new ServiceCollection();
            var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	        provider.Scan(
                services,
                z => z
			        .FromAssemblies()
			        .AddClasses(x => x.AssignableTo<IService>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
            return services;
        }
    }
}
"
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Handle_Public_Open_Generic_Types()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService<T>
{

}

public class Service<T> : IService<T>
{

}

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableTo(typeof(IService<>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Handle_Private_Open_Generic_Types()
    {
        var dependencies = new List<GeneratorTestResults>();

        var rootGenerator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("RootDependencyProject")
                                 .WithAssemblyLoadContext(AssemblyLoadContext)
                                 .AddSources(
                                      @"
namespace RootDependencyProject
{
    public interface IService<T> { }
    class Service<T> : IService<T> { }
}
"
                                  )
                                 .Build()
                                 .GenerateAsync();
        dependencies.Add(rootGenerator);


        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;
using RootDependencyProject;

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableTo(typeof(IService<>)))
                .AsSelf()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Handle_Public_Closed_Generic_Types()
    {
        var dependencies = new List<GeneratorTestResults>();
        var rootGenerator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("RootDependencyProject")
                                 .WithAssemblyLoadContext(AssemblyLoadContext)
                                 .AddSources(
                                      @"
namespace RootDependencyProject
{
    public interface IRequest<T> { }
    public interface IRequestHandler<T, R> where T : IRequest<R> { }
    public class Request : IRequest<Response> { }

    public class Response { }
    public class RequestHandler : IRequestHandler<Request, Response> { }
}
"
                                  )
                                 .Build()
                                 .GenerateAsync();
        dependencies.Add(rootGenerator);

        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;
using RootDependencyProject;

namespace TestProject
{
    public static class Program
    {
        static void Main() { }
        static IServiceCollection LoadServices()
        {
            var services = new ServiceCollection();
            var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	        provider.Scan(
                services,
                z => z
			        .FromAssemblies()
			        .AddClasses(x => x.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
            return services;
        }
    }
}
"
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Handle_Private_Closed_Generic_Types()
    {
        var dependencies = new List<GeneratorTestResults>();
        var rootGenerator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("RootDependencyProject")
                                 .WithAssemblyLoadContext(AssemblyLoadContext)
                                 .AddSources(
                                      @"
namespace RootDependencyProject
{
    public interface IRequest<T> { }
    public interface IRequestHandler<T, R> where T : IRequest<R> { }
    class Request : IRequest<Response> { }

    class Response { }
    class RequestHandler : IRequestHandler<Request, Response> { }
}
"
                                  )
                                 .Build()
                                 .GenerateAsync();
        dependencies.Add(rootGenerator);

        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;
using RootDependencyProject;

namespace TestProject
{
    public static class Program
    {
        static void Main() { }
        static IServiceCollection LoadServices()
        {
            var services = new ServiceCollection();
            var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	        provider.Scan(
                services,
                z => z
			        .FromAssemblies()
			        .AddClasses(x => x.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
            return services;
        }
    }
}
"
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Ignore_Abstract_Classes()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public class Service : IService { }
public abstract class ServiceB : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableTo(typeof(IService)))
                .AsSelf()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Using_Support_As_Type()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public class Service : IService { }
public abstract class ServiceB : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableTo(typeof(IService)))
                .As<IService>()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Handle_Private_Classes_Within_Self()
    {
        var dependency = await GeneratorTestContextBuilder
                              .Create()
                              .WithProjectName("DependencyProject")
                              .WithAssemblyLoadContext(AssemblyLoadContext)
                              .AddSources(
                                   @"
namespace DependencyProject
{
    public interface IService { }
    class Service : IService { }
}
"
                               )
                              .Build()
                              .GenerateAsync();

        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;
using DependencyProject;

namespace TestProject
{
    class Service : IService { }
    public static class Program
    {
        static void Main() { }
        static IServiceCollection LoadServices()
        {
            var services = new ServiceCollection();
            var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	        provider.Scan(
                services,
                z => z
			        .FromAssemblies()
			        .AddClasses(x => x.AssignableTo<IService>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
            return services;
        }
    }
}
"
                           )
                          .AddCompilationReferences(dependency)
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Split_Correctly_Given_Same_Line_Number_Run()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService;
public class Service : IService;
public interface IServiceB;
public class ServiceB : IServiceB;
",
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public static class Program {
    static ServiceCollection Services = new ServiceCollection();
    static void Main() {}
    static IServiceCollection Method()
    {
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            Services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableTo(typeof(IService)), false)
                .AsSelf()
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );

        return Services;
    }
}
",
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public static class Program2 {
    static ServiceCollection Services = new ServiceCollection();

    static IServiceCollection Method()
    {
        var provider = typeof(Program2).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            Services,
            z => z
			.FromAssemblies()
			.AddClasses(x => x.AssignableTo<IServiceB>(), false)
            .AsSelf()
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        return Services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


        var services1 = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "Method");
        var services2 = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program2", "Method");

        await Verify(new GeneratorTestResultsWithServices(result, services1.Concat(services2)));
    }

    [Fact]
    public async Task Should_Filter_AssignableTo()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public interface IServiceB { }
public class Service : IService, IServiceB { }
public class ServiceA : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableTo(typeof(IService)).AssignableTo<IServiceB>())
                .AsSelf()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();

        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_AssignableToAny()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public interface IServiceB { }
public class Service : IService, IServiceB { }
public class ServiceA : IService { }
public class ServiceB : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IService), typeof(IServiceB)))
                .AsSelf()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_AssignableToAny_And_Filter_Interfaces()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public interface IServiceB { }
public class Service : IService, IServiceB { }
public class ServiceA : IService { }
public class ServiceB : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IService), typeof(IServiceB)))
                .AsSelf()
                .AsImplementedInterfaces(z => z.AssignableTo<IServiceB>())
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_AssignableToAny_And_Filter_Interfaces2()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public interface IServiceB { }
public class Service : IService, IServiceB { }
public class ServiceA : IService { }
public class ServiceB : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IService), typeof(IServiceB)))
                .AsImplementedInterfaces(z => z.AssignableTo<IServiceB>())
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }


    [Fact]
    public async Task Should_Filter_AssignableToAny_And_Filter_Generic_Interfaces()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService<T> { }
public interface IOther { }
public class Service : IService<int>, IService<string>, IOther { }
public class ServiceA : IService<string>, IOther { }
public class ServiceB : IService<decimal>, IOther { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IOther)))
                .AsSelf()
                .AsImplementedInterfaces(z => z.AssignableTo(typeof(IService<>)))
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }


    [Fact]
    public async Task Should_Filter_AssignableToAny_And_Filter_Generic_Interfaces2()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService<T> { }
public interface IOther { }
public class Service : IService<int>, IService<string>, IOther { }
public class ServiceA : IService<string>, IOther { }
public class ServiceB : IService<decimal>, IOther { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IOther)))
                .AsImplementedInterfaces(z => z.AssignableTo(typeof(IService<>)))
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }


    [Fact]
    public async Task Should_Filter_As_And_Filter_Interfaces()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public interface IServiceB { }
public class Service : IService, IServiceB { }
public class ServiceA : IService { }
public class ServiceB : IServiceB { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IService)))
                .As<IService>()
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }


    [Fact]
    public async Task Should_Filter_As_And_Filter_Generic_Interfaces()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService<T> { }
public interface IOther { }
public class Service : IService<int>, IService<string>, IOther { }
public class ServiceA : IService<string>, IOther { }
public class ServiceB : IService<decimal> { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IOther)))
                .AsSelf()
                .As(typeof(IService<>))
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_As_And_Filter_Generic_Interfaces2()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService<T> { }
public interface IOther { }
public class Service : IService<int>, IService<string>, IOther { }
public class ServiceA : IService<string>, IOther { }
public class ServiceB : IService<decimal> { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z
			    .FromAssemblies()
			    .AddClasses(x => x.AssignableToAny(typeof(IOther)))
                .As(typeof(IService<>))
                .WithScopedLifetime()
        );
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();


//        await Verify(result);
        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Support_ServiceRegistrationAttributes()
    {
        var result = await Builder
                          .AddSources(
                               @"
using System;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public interface IServiceB { }
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IServiceB))]
public class Service : IService, IServiceB { }
[ServiceRegistration(ServiceLifetime.Transient)]
public class ServiceA : IService { }
[ServiceRegistration]
public class ServiceB : IService, IServiceB { }
"
                           )
                          .Build()
                          .GenerateAsync();

        await Verify(result);
    }

    [Fact]
    public async Task Should_Report_Diagnostic_When_Not_Using_Expressions()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public class Service : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => {
                   z.FromAssemblies()
			        .AddClasses(x => x.AssignableTo(typeof(IService)))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
        });
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();

        await Verify(result);
    }

    [Fact]
    public async Task Should_Report_Diagnostic_Not_Given_A_Compiled_Type()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public class Service : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var type = typeof(IService);
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z.FromAssemblies()
			  .AddClasses(x => x.AssignableTo(type))
              .AsSelf()
              .AsImplementedInterfaces()
              .WithScopedLifetime());
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();

        await Verify(result);
    }

    [Fact]
    public async Task Should_Report_Diagnostic_Not_Given_A_Static_Namespace()
    {
        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
public class Service : IService { }

public static class Program {
    static void Main() { }
    static IServiceCollection LoadServices()
    {
        var ns = ""MyNamespace"";
        var services = new ServiceCollection();
        var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	    provider.Scan(
            services,
            z => z.FromAssemblies()
			  .AddClasses(x => x.InNamespaces(ns))
              .AsSelf()
              .AsImplementedInterfaces()
              .WithScopedLifetime());
        return services;
    }
}
"
                           )
                          .Build()
                          .GenerateAsync();

        await Verify(result);
    }

    [Fact]
    public async Task Should_Report_Diagnostic_For_Duplicate_ServiceRegistrationAttributes()
    {
        var result = await Builder
                          .AddSources(
                               @"
using System;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;

public interface IService { }
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IService))]
[ServiceRegistration<IService>(ServiceLifetime.Singleton)]
public class Service : IService { }
"
                           )
                          .Build()
                          .GenerateAsync();

        await Verify(result);
    }

    [Fact]
    public async Task Should_Filter_With_EndsWith()
    {
        var result = await Builder
                          .AddSources(
                               """

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface Factory {}
                               public interface IService { }
                               public interface IServiceB { }
                               public class ServiceFactory : IService, IServiceB { }
                               public class ServiceA : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo(typeof(IService)).AssignableTo<IServiceB>().EndsWith("Factory"))
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_With_EndsWith_NameOf()
    {
        var result = await Builder
                          .AddSources(
                               """

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface Factory {}
                               public interface IService { }
                               public interface IServiceB { }
                               public class ServiceFactory : IService, IServiceB { }
                               public class ServiceA : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo(typeof(IService)).AssignableTo<IServiceB>().EndsWith(nameof(Factory)))
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_With_StartsWith()
    {
        var result = await Builder
                          .AddSources(
                               """

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface Factory {}
                               public interface IService { }
                               public interface IServiceB { }
                               public class FactoryService : IService, IServiceB { }
                               public class ServiceA : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo(typeof(IService)).AssignableTo<IServiceB>().StartsWith("Factory"))
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_With_StartsWith_NameOf()
    {
        var result = await Builder
                          .AddSources(
                               """

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface Factory {}
                               public interface IService { }
                               public interface IServiceB { }
                               public class FactoryService : IService, IServiceB { }
                               public class ServiceA : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo(typeof(IService)).AssignableTo<IServiceB>().StartsWith(nameof(Factory)))
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_With_Contains()
    {
        var result = await Builder
                          .AddSources(
                               """

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface Factory {}
                               public interface IService { }
                               public interface IServiceB { }
                               public class ServFactoryice : IService, IServiceB { }
                               public class ServiceA : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo(typeof(IService)).AssignableTo<IServiceB>().Contains("Factory"))
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Fact]
    public async Task Should_Filter_With_Contains_NameOf()
    {
        var result = await Builder
                          .AddSources(
                               """

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface Factory {}
                               public interface IService { }
                               public interface IServiceB { }
                               public class ServFactoryice : IService, IServiceB { }
                               public class ServiceA : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo(typeof(IService)).AssignableTo<IServiceB>().Contains(nameof(Factory)))
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Should_Handle_Private_Generic_Classes_Within_Multiple_Dependencies(int dependencyCount)
    {
        var dependencies = new List<GeneratorTestResults>();
        var rootGenerator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("RootDependencyProject")
                                 .WithAssemblyLoadContext(AssemblyLoadContext)
                                 .AddSources(
                                      @"
namespace RootDependencyProject
{
    public interface IRequest<T> { }
    public interface IRequestHandler<T, R> where T : IRequest<R> { }
}
"
                                  )
                                 .Build()
                                 .GenerateAsync();
        dependencies.Add(rootGenerator);

        for (var i = 0; i < dependencyCount; i++)
        {
            var dependency = await GeneratorTestContextBuilder
                                  .Create()
                                  .WithProjectName($"Dependency{i}Project")
                                  .WithAssemblyLoadContext(AssemblyLoadContext)
                                  .AddCompilationReferences(rootGenerator)
                                  .AddSources(
                                       $$"""

                                       using RootDependencyProject;

                                       namespace Dependency{{1}}Project
                                       {
                                           {{( i % 2 == 0 ? "public" : "" )}} class Request{{i}} : IRequest<Response{{i}}> { }
                                           {{( i % 2 == 0 ? "public" : "" )}} class Response{{i}} { }
                                           {{( i % 2 == 0 ? "public" : "" )}} class RequestHandler{{i}} : IRequestHandler<Request{{i}}, Response{{i}}>  { }
                                       }

                                       """
                                   )
                                  .Build()
                                  .GenerateAsync();
            dependencies.Add(dependency);
        }


        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;
using RootDependencyProject;

namespace TestProject
{
    public static class Program
    {
        static void Main() { }
        static IServiceCollection LoadServices()
        {
            var services = new ServiceCollection();
            var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	        provider.Scan(
                services,
                z => z
			        .FromAssemblies()
			        .AddClasses(x => x.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
            return services;
        }
    }
}
"
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).UseParameters(dependencyCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Should_Handle_Private_Classes_Within_Multiple_Dependencies(int dependencyCount)
    {
        var dependencies = new List<GeneratorTestResults>();
        var rootGenerator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("RootDependencyProject")
                                 .WithAssemblyLoadContext(AssemblyLoadContext)
                                 .AddSources(
                                      @"
namespace RootDependencyProject
{
    public interface IService { }
}
"
                                  )
                                 .Build()
                                 .GenerateAsync();
        dependencies.Add(rootGenerator);

        for (var i = 0; i < dependencyCount; i++)
        {
            var dependency = await GeneratorTestContextBuilder
                                  .Create()
                                  .WithProjectName($"Dependency{i}Project")
                                  .WithAssemblyLoadContext(AssemblyLoadContext)
                                  .AddCompilationReferences(rootGenerator)
                                  .AddSources(
                                       $$"""

                                       namespace Dependency{{1}}Project
                                       {
                                           class Service{{i}} : RootDependencyProject.IService { }
                                       }

                                       """
                                   )
                                  .Build()
                                  .GenerateAsync();
            dependencies.Add(dependency);
        }


        var result = await Builder
                          .AddSources(
                               @"
using Rocket.Surgery.DependencyInjection.Compiled;
using Microsoft.Extensions.DependencyInjection;
using RootDependencyProject;

namespace TestProject
{
    public static class Program
    {
        static void Main() { }
        static IServiceCollection LoadServices()
        {
            var services = new ServiceCollection();
            var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
	        provider.Scan(
                services,
                z => z
			        .FromAssemblies()
			        .AddClasses(x => x.AssignableTo<IService>())
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
            return services;
        }
    }
}
"
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).UseParameters(dependencyCount);
    }

    [Theory]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Transient)]
    public async Task Should_Have_Correct_Lifetime(ServiceLifetime serviceLifetime)
    {
        var result = await Builder
                          .AddSources(
                               $$"""

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface IService;
                               public class Service : IService;
                               public interface IServiceB;
                               public class ServiceB : IServiceB;

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                                       provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo<IService>())
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .With{{serviceLifetime}}Lifetime()
                                       );
                               
                               	        provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo<IServiceB>(), false)
                                               .AsSelf()
                                               .AsMatchingInterface()
                                               .WithLifetime(ServiceLifetime.{{serviceLifetime}})
                                       );
                                       return services;
                                   }
                               }
                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).UseParameters(serviceLifetime);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_Filter_WithAttribute(bool useTypeof)
    {
        var result = await Builder
                          .AddSources(
                               $$"""

                               using System;
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public class MyAttribute : Attribute { }
                               public interface IService { }
                               public interface IServiceB { }
                               public class Service : IService, IServiceB { }
                               [MyAttribute]
                               public class ServiceA : IService { }
                               public class ServiceB : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.WithAttribute{{( useTypeof ? "(typeof(MyAttribute))" : "<MyAttribute>()" )}})
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).UseParameters(useTypeof);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_Filter_WithoutAttribute(bool useTypeof)
    {
        var result = await Builder
                          .AddSources(
                               $$"""

                               using System;
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public class MyAttribute : Attribute { }
                               public interface IService { }
                               public interface IServiceB { }
                               public class Service : IService, IServiceB { }
                               [MyAttribute]
                               public class ServiceA : IService { }
                               public class ServiceB : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.AssignableTo(typeof(IService)).WithoutAttribute{{( useTypeof ? "(typeof(MyAttribute))" : "<MyAttribute>()" )}})
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).UseParameters(useTypeof);
    }

    [Theory]
    [InlineData(NamespaceFilter.Exact, "TestProject.A", 5, false, false)]
    [InlineData(NamespaceFilter.Exact, "TestProject.A.IService", 5, true, false)]
    [InlineData(NamespaceFilter.Exact, "TestProject.A.IService", 5, true, true)]
    [InlineData(NamespaceFilter.In, "TestProject.A", 7, false, false)]
    [InlineData(NamespaceFilter.In, "TestProject.A.IService", 7, true, false)]
    [InlineData(NamespaceFilter.In, "TestProject.A.IService", 7, true, true)]
    [InlineData(NamespaceFilter.NotIn, "TestProject.A.C", 7, false, false)]
    [InlineData(NamespaceFilter.NotIn, "TestProject.A.C.ServiceC", 7, true, false)]
    [InlineData(NamespaceFilter.NotIn, "TestProject.A.C.ServiceC", 7, true, true)]
    public async Task Should_Filter_Namespaces(NamespaceFilter filter, string namespaceFilterValue, int count, bool usingClass, bool usingTypeof)
    {
        var result = await Builder
                          .AddSources(
                               $$"""

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               namespace TestProject.A
                               {
                                   public interface IService { }
                                   public class Service : IService, TestProject.B.IServiceB { }
                                   public class ServiceA : IService { }
                               }

                               namespace TestProject.A.C
                               {
                                   public class ServiceC : IService { }
                               }

                               namespace TestProject.B
                               {
                                   public interface IServiceB { }
                                   public class ServiceB : TestProject.A.IService { }
                               }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	    provider.Scan(
                                           services,
                                       z => z
                               			.FromAssemblies()
                               			.AddClasses(x => x.{{( usingClass, usingTypeof, filter ) switch
                                                                {
                                                                    (false, false, NamespaceFilter.Exact) => $"InExactNamespaces(\"{namespaceFilterValue}\")",
                                                                    (false, false, NamespaceFilter.In) => $"InNamespaces(\"{namespaceFilterValue}\")",
                                                                    (false, false, NamespaceFilter.NotIn) => $"InNamespaces(\"TestProject\").NotInNamespaces(\"{namespaceFilterValue}\")",
                                                                    (true, false, NamespaceFilter.Exact) => $"InExactNamespaceOf(typeof({namespaceFilterValue}))",
                                                                    (true, false, NamespaceFilter.In) => $"InNamespaceOf(typeof({namespaceFilterValue}))",
                                                                    (true, false, NamespaceFilter.NotIn) => $"InNamespaces(\"TestProject\").NotInNamespaceOf(typeof({namespaceFilterValue}))",
                                                                    (true, true, NamespaceFilter.Exact) => $"InExactNamespaceOf<{namespaceFilterValue}>()",
                                                                    (true, true, NamespaceFilter.In) => $"InNamespaceOf<{namespaceFilterValue}>()",
                                                                    (true, true, NamespaceFilter.NotIn) => $"InNamespaces(\"TestProject\").NotInNamespaceOf<{namespaceFilterValue}>()",
                                                                    _ => "ERROR",
                                                                }}})
                                           .AsSelf()
                                           .AsImplementedInterfaces()
                                           .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
             .HashParameters()
             .UseParameters(filter, namespaceFilterValue, count, usingClass, usingTypeof);
    }

    [Theory]
    [InlineData(NamespaceFilter.Exact, "TestProject.A", "TestProject.B", 7, false)]
    [InlineData(NamespaceFilter.Exact, "TestProject.A.ServiceA", "TestProject.B.ServiceB", 7, true)]
    [InlineData(NamespaceFilter.In, "TestProject.A", "TestProject.B", 9, false)]
    [InlineData(NamespaceFilter.In, "TestProject.A.ServiceA", "TestProject.B.ServiceB", 9, true)]
    [InlineData(NamespaceFilter.NotIn, "TestProject.A.C", "TestProject.B", 5, false)]
    [InlineData(NamespaceFilter.NotIn, "TestProject.A.C.ServiceC", "TestProject.B.ServiceB", 5, true)]
    public async Task Should_Filter_Multiple_Namespaces(
        NamespaceFilter filter,
        string namespaceFilterValue,
        string namespaceFilterValueSecond,
        int count,
        bool usingClass
    )
    {
        var result = await Builder
                          .AddSources(
                               $$"""

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               namespace TestProject.A
                               {
                                   public interface IService { }
                                   public class Service : IService, TestProject.B.IServiceB { }
                                   public class ServiceA : IService { }
                               }

                               namespace TestProject.A.C
                               {
                                   public class ServiceC : IService { }
                               }

                               namespace TestProject.B
                               {
                                   public interface IServiceB { }
                                   public class ServiceB : TestProject.A.IService { }
                               }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	        provider.Scan(
                                           services,
                                           z => z
                               			    .FromAssemblies()
                               			    .AddClasses(x => x.{{( usingClass, filter ) switch
                                                                    {
                                                                        (false, NamespaceFilter.Exact) => $"InExactNamespaces(\"{namespaceFilterValue}\", \"{namespaceFilterValueSecond}\")",
                                                                        (false, NamespaceFilter.In) => $"InNamespaces(\"{namespaceFilterValue}\", \"{namespaceFilterValueSecond}\")",
                                                                        (false, NamespaceFilter.NotIn) => $"InNamespaces(\"TestProject\").NotInNamespaces(\"{namespaceFilterValue}\", \"{namespaceFilterValueSecond}\")",
                                                                        (true, NamespaceFilter.Exact) => $"InExactNamespaceOf(typeof({namespaceFilterValue}), typeof({namespaceFilterValueSecond}))",
                                                                        (true, NamespaceFilter.In) => $"InNamespaceOf(typeof({namespaceFilterValue}), typeof({namespaceFilterValueSecond}))",
                                                                        (true, NamespaceFilter.NotIn) =>
                                                                            $"InNamespaces(\"TestProject\").NotInNamespaceOf(typeof({namespaceFilterValue}), typeof({namespaceFilterValueSecond}))",
                                                                        _ => "ERROR",
                                                                    }}})
                                               .AsSelf()
                                               .AsImplementedInterfaces()
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }

                               """
                           )
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).HashParameters().UseParameters(filter, namespaceFilterValue, count, usingClass);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_Select_Specific_Assemblies_Using_FromAssemblyOf(bool useTypeof)
    {
        var dependencies = new List<GeneratorTestResults>();

        static async Task<GeneratorTestResults> CreateServiceDependency(
            AssemblyLoadContext context,
            ILogger logger,
            string suffix,
            params GeneratorTestResults[] dependencies
        )
        {
            var generator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("DependencyProject" + suffix)
                                 .AddCompilationReferences(dependencies)
                                 .WithLogger(logger)
                                 .WithAssemblyLoadContext(context)
                                 .AddSources(
                                      $$"""

                                      namespace DependencyProject{{suffix}}
                                      {
                                          public interface IService{{suffix}} { }
                                          public class Service{{suffix}} : IService{{suffix}} { }
                                      }

                                      """
                                  )
                                 .Build()
                                 .GenerateAsync();
            generator.AssertCompilationWasSuccessful();
            generator.AssertGenerationWasSuccessful();
            return generator;
        }

        var dependencyA = await CreateServiceDependency(AssemblyLoadContext, Logger, "A");
        var dependencyB = await CreateServiceDependency(AssemblyLoadContext, Logger, "B");
        var dependencyC = await CreateServiceDependency(AssemblyLoadContext, Logger, "C", dependencyA);
        var dependencyD = await CreateServiceDependency(AssemblyLoadContext, Logger, "D", dependencyC);
        dependencies.Add(dependencyA);
        dependencies.Add(dependencyB);
        dependencies.Add(dependencyC);
        dependencies.Add(dependencyD);

        var result = await Builder
                          .AddSources(
                               $$"""

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;
                               using DependencyProjectA;
                               using DependencyProjectB;
                               using DependencyProjectC;
                               using DependencyProjectD;

                               namespace TestProject
                               {
                                   public static class Program
                                   {
                                       static void Main() { }
                                       static IServiceCollection LoadServices()
                                       {
                                           var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	        provider.Scan(
                                               services,
                                               z => z
                                                   .FromAssemblyOf{{( useTypeof ? "(typeof(IServiceB))" : "<IServiceB>()" )}}
                                                   .AddClasses()
                                                   .AsSelf()
                                                   .AsImplementedInterfaces()
                                                   .WithSingletonLifetime()
                                           );
                                           return services;
                                       }
                                   }
                               }

                               """
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).UseParameters(useTypeof);
    }

    [Theory]
    [InlineData("ServiceA", false, 2)]
    [InlineData("ServiceB", false, 0)]
    [InlineData("ServiceC", false, 1)]
    [InlineData("ServiceA", true, 2)]
    [InlineData("ServiceB", true, 0)]
    [InlineData("ServiceC", true, 1)]
    public async Task Should_Select_Specific_Assemblies_Using_FromAssemblyDependenciesOf(string className, bool useTypeof, int expectedCount)
    {
        var dependencies = new List<GeneratorTestResults>();

        static async Task<GeneratorTestResults> CreateRoot(
            AssemblyLoadContext context,
            ILogger logger,
            params GeneratorTestResults[] dependencies
        )
        {
            var generator = await GeneratorTestContextBuilder
                                 .Create()
                                 .WithProjectName("RootDependencyProject")
                                 .WithAssemblyLoadContext(context)
                                 .WithLogger(logger)
                                 .AddCompilationReferences(dependencies)
                                 .AddSources(
                                      @"
namespace RootDependencyProject
{
    public interface IService { }
}
"
                                  )
                                 .Build()
                                 .GenerateAsync();
            generator.AssertCompilationWasSuccessful();
            generator.AssertGenerationWasSuccessful();
            return generator;
        }

        static async Task<GeneratorTestResults> CreateServiceDependency(
            AssemblyLoadContext context,
            ILogger logger,
            string suffix,
            params GeneratorTestResults[] dependencies
        )
        {
            var additionalCode = dependencies
                                .Where(z => z.FinalCompilation.AssemblyName?.StartsWith("DependencyProject") == true)
                                .Select(
                                     z =>
                                         $"class HardReference{z.FinalCompilation.AssemblyName?.Substring(z.FinalCompilation.AssemblyName.Length - 1)} : {z.FinalCompilation.AssemblyName + ".Service" + z.FinalCompilation.AssemblyName?.Substring(z.FinalCompilation.AssemblyName.Length - 1)} {{ }}"
                                 );
            var dep = await GeneratorTestContextBuilder
                           .Create()
                           .WithProjectName("DependencyProject" + suffix)
                           .WithAssemblyLoadContext(context)
                           .WithLogger(logger)
                           .AddCompilationReferences(dependencies)
                           .AddSources(
                                $$"""

                                namespace DependencyProject{{suffix}}
                                {
                                    {{string.Join("\n", additionalCode)}}
                                    public class Service{{suffix}} : RootDependencyProject.IService { }
                                }

                                """
                            )
                           .Build()
                           .GenerateAsync();
            return dep;
        }

        var root = await CreateRoot(AssemblyLoadContext, Logger);
        var dependencyA = await CreateServiceDependency(AssemblyLoadContext, Logger, "A", root);
        var dependencyB = await CreateServiceDependency(AssemblyLoadContext, Logger, "B", root);
        var dependencyC = await CreateServiceDependency(AssemblyLoadContext, Logger, "C", dependencyA, root);
        var dependencyD = await CreateServiceDependency(
            AssemblyLoadContext,
            Logger,
            "D",
            dependencyA,
            dependencyC,
            root
        );
        dependencies.Add(root);
        dependencies.Add(dependencyA);
        dependencies.Add(dependencyB);
        dependencies.Add(dependencyC);
        dependencies.Add(dependencyD);

        var result = await Builder
                          .AddSources(
                               $$"""


                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;
                               using RootDependencyProject;
                               using DependencyProjectA;
                               using DependencyProjectB;
                               using DependencyProjectC;
                               using DependencyProjectD;

                               namespace TestProject
                               {
                                   public static class Program
                                   {
                                       static void Main() { }
                                       static IServiceCollection LoadServices()
                                       {
                                           var services = new ServiceCollection();
                                           var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                               	            provider.Scan(
                                               services,
                                               z => z
                               			        .FromAssemblyDependenciesOf{{( useTypeof ? $"(typeof({className}))" : $"<{className}>()" )}}
                                                   .AddClasses(x => x.AssignableTo(typeof(IService)), true)
                                                   .AsSelf()
                                                   .AsImplementedInterfaces()
                                                   .WithSingletonLifetime()
                                           );
                                           return services;
                                       }
                                   }
                               }

                               """
                           )
                          .AddCompilationReferences(dependencies.ToArray())
                          .Build()
                          .GenerateAsync();


        var services = StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services)).UseParameters(className, useTypeof, expectedCount);
    }

    private static class StaticHelper
    {
        public static IServiceCollection ExecuteStaticServiceCollectionMethod(Assembly? assembly, string className, string methodName)
        {
            if (assembly == null) return new ServiceCollection();
            var @class = assembly.GetTypes().FirstOrDefault(z => z.IsClass && z.Name == className)!;
            Assert.NotNull(@class);

            var method = @class.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(method);

            return ( method!.Invoke(null, Array.Empty<object>()) as IServiceCollection )!;
        }
    }
}
