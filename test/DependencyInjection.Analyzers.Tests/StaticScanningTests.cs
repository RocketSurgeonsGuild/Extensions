using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

using Microsoft.Extensions.DependencyInjection;

using Rocket.Surgery.Extensions.Testing.SourceGenerators;

using Serilog;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

[Timeout(ModuleInitializer.TestTimeout)]
public class StaticScanningTests : GeneratorTest
{
    [Test]
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
public static class Nested
{
    public class ServiceA : IService<string>, IOther { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService<string>, IOther { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService { }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService<string>, IOther { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService<string>, IOther { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
    public async Task Should_Filter_AssignableToAny_And_Filter_Generic_Interfaces3()
    {
        var result = await Builder
                          .AddSources(
                               // ReSharper disable once HeapView.ObjectAllocation
                               """
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface IValidator { }
                               public interface IValidator<T> : IValidator { }

                               public static class Nested
                               {
                                   public record MyRecord();
                                   public class Validator : IValidator<MyRecord> { }
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
                               			    .AddClasses(x => x.AssignableToAny(typeof(IValidator)))
                                               .AsImplementedInterfaces(z => z.AssignableTo(typeof(IValidator<>)))
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }
                               """
                           )
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
    public async Task Should_Filter_AssignableToAny_And_Filter_Generic_Interfaces4()
    {
        var result = await Builder
                          .AddSources(
                               // ReSharper disable once HeapView.ObjectAllocation
                               """
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface IValidator { }
                               public interface IValidator<T> : IValidator { }

                               public static class Nested
                               {
                                   public record MyRecord();
                                   private class Validator : IValidator<MyRecord> { }
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
                               			    .AddClasses(x => x.AssignableToAny(typeof(IValidator)))
                                               .AsImplementedInterfaces(z => z.AssignableTo(typeof(IValidator<>)))
                                               .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }
                               """
                           )
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
    public async Task Should_Filter_AssignableToAny_And_Filter_Generic_Interfaces5()
    {
        var result = await Builder
                          .AddSources(
                               // ReSharper disable once HeapView.ObjectAllocation
                               """
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface IValidator { }
                               public interface IValidator<T> : IValidator { }

                               public static class Nested
                               {
                                   private record MyRecord();
                                   private class Validator : IValidator<MyRecord> { }
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
                               			    .AddClasses(x => x.AssignableToAny(typeof(IValidator)))
                               			    .AsSelf()
                                            .AsImplementedInterfaces(z => z.AssignableTo(typeof(IValidator<>)))
                                            .WithScopedLifetime()
                                       );
                                       return services;
                                   }
                               }
                               """
                           )
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
public static class Nested
{
    public class ServiceA : IService { }
}
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
    [Arguments(NamespaceFilter.Exact, "TestProject.A", "TestProject.B", false)]
    [Arguments(NamespaceFilter.Exact, "TestProject.A.Nested.ServiceA", "TestProject.B.ServiceB", true)]
    [Arguments(NamespaceFilter.In, "TestProject.A", "TestProject.B", false)]
    [Arguments(NamespaceFilter.In, "TestProject.A.Nested.ServiceA", "TestProject.B.ServiceB", true)]
    [Arguments(NamespaceFilter.NotIn, "TestProject.A.C", "TestProject.B", false)]
    [Arguments(NamespaceFilter.NotIn, "TestProject.A.C.ServiceC", "TestProject.B.ServiceB", true)]
    public async Task Should_Filter_Multiple_Namespaces(NamespaceFilter filter, string namespaceFilterValue, string namespaceFilterValueSecond, bool usingClass)
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
                                   public static class Nested
                                   {
                                       public class ServiceA : IService { }
                                   }
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
                               			    .AddClasses(x => x.{{(usingClass, filter) switch
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
             .HashParameters()
             .UseParameters(filter, namespaceFilterValue, namespaceFilterValueSecond, usingClass)
            ;
    }

    [Test]
    [Arguments(NamespaceFilter.Exact, "TestProject.A", false, false)]
    [Arguments(NamespaceFilter.Exact, "TestProject.A.IService", true, false)]
    [Arguments(NamespaceFilter.Exact, "TestProject.A.IService", true, true)]
    [Arguments(NamespaceFilter.In, "TestProject.A", false, false)]
    [Arguments(NamespaceFilter.In, "TestProject.A.IService", true, false)]
    [Arguments(NamespaceFilter.In, "TestProject.A.IService", true, true)]
    [Arguments(NamespaceFilter.NotIn, "TestProject.A.C", false, false)]
    [Arguments(NamespaceFilter.NotIn, "TestProject.A.C.ServiceC", true, false)]
    [Arguments(NamespaceFilter.NotIn, "TestProject.A.C.ServiceC", true, true)]
    public async Task Should_Filter_Namespaces(NamespaceFilter filter, string namespaceFilterValue, bool usingClass, bool usingTypeof)
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
                                   public static class Nested
                                   {
                                       public class ServiceA : IService { }
                                   }
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
                               			.AddClasses(x => x.{{(usingClass, usingTypeof, filter) switch
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
             .HashParameters()
             .UseParameters(filter, namespaceFilterValue, usingClass, usingTypeof)
            ;
    }

    [Test]
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
                               public static class Nested
                               {
                                   public class ServiceA : IService { }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                               public static class Nested
                               {
                                   public class ServiceA : IService { }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                               public static class Nested
                               {
                                   public class ServiceA : IService { }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                               public static class Nested
                               {
                                   public class ServiceA : IService { }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                               public static class Nested
                               {
                                   public class ServiceA : IService { }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                               public static class Nested
                               {
                                   public class ServiceA : IService { }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
    [MatrixDataSource]
    public async Task Should_Filter_WithAttribute([Matrix(true, false)] bool useTypeof)
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
                               public static class Nested
                               {
                                   [MyAttribute]
                                   public class ServiceA : IService { }
                               }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
               .UseParameters(useTypeof)
            ;
    }

    [Test]
    [MatrixDataSource]
    public async Task Should_Filter_WithoutAttribute([Matrix(true, false)] bool useTypeof)
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
                               public static class Nested
                               {
                                   [MyAttribute]
                                   public class ServiceA : IService { }
                               }
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
               .UseParameters(useTypeof)
            ;
    }

    [Test]
    [MatrixDataSource]
    public async Task Should_Handle_Private_Classes_Within_Multiple_Dependencies(
        [Matrix(
            0,
            1,
            2,
            3,
            4,
            5
        )]
        int dependencyCount
    )
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
                                 .AddCacheOptions(GetTempPath())
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
                                  .AddCacheOptions(GetTempPath())
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
               .UseParameters(dependencyCount)
            ;
    }

    [Test]
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
                              .AddCacheOptions(GetTempPath())
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                                 .AddCacheOptions(GetTempPath())
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Test]
    [MatrixDataSource]
    public async Task Should_Handle_Private_Generic_Classes_Within_Multiple_Dependencies(
        [Matrix(
            0,
            1,
            2,
            3,
            4,
            5
        )]
        int dependencyCount
    )
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
                                 .AddCacheOptions(GetTempPath())
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
                                  .AddCacheOptions(GetTempPath())
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
               .UseParameters(dependencyCount)
            ;
    }

    [Test]
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
                                 .AddCacheOptions(GetTempPath())
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                                 .AddCacheOptions(GetTempPath())
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                                 .AddCacheOptions(GetTempPath())
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services));
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
    [MatrixDataSource]
    public async Task Should_Have_Correct_Lifetime(
        [Matrix(ServiceLifetime.Scoped, ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        ServiceLifetime serviceLifetime
    )
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
               .UseParameters(serviceLifetime)
            ;
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result)
            ;
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result)
            ;
    }

    [Test]
    public async Task Should_Report_Diagnostic_Not_Given_A_Static_Namespace()
    {
        var result = await Builder
                          .AddSources(
                               """

                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public interface IService { }
                               public class Service : IService { }

                               public static class Program {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var ns = "MyNamespace";
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

                               """
                           )
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result)
            ;
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result)
            ;
    }

    [Test]
    [MatrixDataSource]
    public async Task Should_Select_Specific_Assemblies_Using_FromAssemblyDependenciesOf(
        [Matrix("ServiceA", "ServiceB", "ServiceC")]
        string className,
        [Matrix(true, false)]
        bool useTypeof
    )
    {
        var dependencies = new List<GeneratorTestResults>();

        static async Task<GeneratorTestResults> CreateRoot(
            AssemblyLoadContext context,
            ILogger logger,
            string tempPath,
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
                                 .AddGlobalOption("build_property.IntermediateOutputPath", tempPath)
                                 .Build()
                                 .GenerateAsync();
            generator.AssertCompilationWasSuccessful();
            generator.AssertGenerationWasSuccessful();
            return generator;
        }

        static async Task<GeneratorTestResults> CreateServiceDependency(
            AssemblyLoadContext context,
            ILogger logger,
            string tempPath,
            string suffix,
            params GeneratorTestResults[] dependencies
        )
        {
            var additionalCode = dependencies
                                .Where(z => z.FinalCompilation.AssemblyName?.StartsWith("DependencyProject") == true)
                                .Select(
                                     z =>
                                         $"class HardReference{z.FinalCompilation.AssemblyName?[^1..]} : {z.FinalCompilation.AssemblyName + ".Service" + z.FinalCompilation.AssemblyName?[^1..]} {{ }}"
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
                           .AddGlobalOption("build_property.IntermediateOutputPath", tempPath)
                           .Build()
                           .GenerateAsync();
            return dep;
        }

        var root = await CreateRoot(AssemblyLoadContext, Logger, GetTempPath());
        var dependencyA = await CreateServiceDependency(AssemblyLoadContext, Logger, GetTempPath(), "A", root);
        var dependencyB = await CreateServiceDependency(AssemblyLoadContext, Logger, GetTempPath(), "B", root);
        var dependencyC = await CreateServiceDependency(
            AssemblyLoadContext,
            Logger,
            GetTempPath(),
            "C",
            dependencyA,
            root
        );
        var dependencyD = await CreateServiceDependency(
            AssemblyLoadContext,
            Logger,
            GetTempPath(),
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
               .UseParameters(className, useTypeof)
            ;
    }

    [Test]
    [MatrixDataSource]
    public async Task Should_Select_Specific_Assemblies_Using_FromAssemblyOf([Matrix(true, false)] bool useTypeof)
    {
        var dependencies = new List<GeneratorTestResults>();

        static async Task<GeneratorTestResults> CreateServiceDependency(
            AssemblyLoadContext context,
            ILogger logger,
            string tempPath,
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
                                 .AddGlobalOption("build_property.IntermediateOutputPath", tempPath)
                                 .Build()
                                 .GenerateAsync();
            generator.AssertCompilationWasSuccessful();
            generator.AssertGenerationWasSuccessful();
            return generator;
        }

        var dependencyA = await CreateServiceDependency(AssemblyLoadContext, Logger, GetTempPath(), "A");
        var dependencyB = await CreateServiceDependency(AssemblyLoadContext, Logger, GetTempPath(), "B");
        var dependencyC = await CreateServiceDependency(AssemblyLoadContext, Logger, GetTempPath(), "C", dependencyA);
        var dependencyD = await CreateServiceDependency(AssemblyLoadContext, Logger, GetTempPath(), "D", dependencyC);
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
                          .AddCompilationReferences([.. dependencies])
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
               .UseParameters(useTypeof)
            ;
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services1 = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "Method");
        var services2 = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program2", "Method");

        await Verify(new GeneratorTestResultsWithServices(result, services1.Concat(services2)))
            ;
    }

    [Test]
    public async Task Should_Support_ServiceRegistrationAttributes()
    {
        var result = await Builder
                          .AddSource(
                               "CompiledTypeProviderServiceCollectionExtensions.cs",
                               """
                               using Rocket.Surgery.DependencyInjection;
                               using Rocket.Surgery.DependencyInjection.Compiled;

                               // ReSharper disable once CheckNamespace
                               namespace Microsoft.Extensions.DependencyInjection;

                               /// <summary>
                               ///     Extension methods for the service collection using the compiled type provider
                               /// </summary>
                               public static class CompiledTypeProviderServiceCollectionExtensions
                               {
                                   /// <summary>
                                   ///     Adds all the services with the <see cref="ServiceRegistrationAttribute" /> to the service collection
                                   /// </summary>
                                   /// <param name="services"></param>
                                   /// <param name="provider"></param>
                                   /// <returns></returns>
                                   public static IServiceCollection AddCompiledServiceRegistrations(this IServiceCollection services, ICompiledTypeProvider provider)
                                   {
                                       // This is implied to ignore abstract and static classes.
                                       return provider.Scan(
                                           services,
                                           z => z
                                               .FromAssemblies()
                                               .AddClasses(
                                                    f => f
                                                       .WithAnyAttribute(
                                                            typeof(ServiceRegistrationAttribute),
                                                            typeof(ServiceRegistrationAttribute<,>),
                                                            typeof(ServiceRegistrationAttribute<,,>),
                                                            typeof(ServiceRegistrationAttribute<,,>),
                                                            typeof(ServiceRegistrationAttribute<,,,>)
                                                        )
                                                )
                                               .AsSelf()
                                               .WithSingletonLifetime()
                                       );
                                   }
                               }
                               """
                           )
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
public static class Nested
{
    [ServiceRegistration(ServiceLifetime.Transient)]
    public class ServiceA : IService { }
}
[ServiceRegistration]
public class ServiceB : IService, IServiceB { }
"
                           )
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result)
            ;
    }

    [Test]
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
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        var services = await StaticHelper.ExecuteStaticServiceCollectionMethod(result, "Program", "LoadServices");
        await Verify(new GeneratorTestResultsWithServices(result, services))
            ;
    }

    [Test]
    public async Task Should_Work_With_this_Weird_Query()
    {
        var rootGenerator = await Builder
                                 .WithProjectName("RootDependencyProject")
                                 .AddSources(
                                      """
                                      using System;
                                      using Rocket.Surgery.DependencyInjection.Compiled;
                                      using Microsoft.Extensions.DependencyInjection;

                                      [AttributeUsage(AttributeTargets.Class)]
                                      public sealed class RegisterOptionsConfigurationAttribute(string configurationKey) : Attribute
                                      {
                                          /// <summary>
                                          ///     The configuration key to use
                                          /// </summary>
                                          public string ConfigurationKey { get; } = configurationKey;

                                          /// <summary>
                                          ///     The optional options name
                                          /// </summary>
                                          public string? OptionsName { get; set; }
                                      }

                                      public static class Program {
                                          static void Main() { }
                                          static IServiceCollection LoadServices()
                                          {
                                              var services = new ServiceCollection();
                                              var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                                              var classes = provider.GetTypes(
                                                  s => s.FromAssemblyDependenciesOf<RegisterOptionsConfigurationAttribute>().GetTypes(f => f.WithAttribute<RegisterOptionsConfigurationAttribute>())
                                              );

                                              return services;
                                          }
                                      }

                                      """
                                  )
                                 .AddCacheOptions(GetTempPath())
                                 .Build()
                                 .GenerateAsync();

        var result = await Builder
                          .AddSources(
                               """
                               [RegisterOptionsConfiguration("OptionsA")]
                               class OptionsA
                               {
                                   public required string A { get; set; }
                               }

                               [RegisterOptionsConfiguration("OptionsB")]
                               class OptionsB
                               {
                                   public required string B { get; set; }
                               }
                               """
                           )
                          .AddSources(
                               """
                               using System;
                               using Rocket.Surgery.DependencyInjection.Compiled;
                               using Microsoft.Extensions.DependencyInjection;

                               public static class Program2 {
                                   static void Main() { }
                                   static IServiceCollection LoadServices()
                                   {
                                       var services = new ServiceCollection();
                                       var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                                       var classes = provider.GetTypes(
                                           s => s.FromAssemblyDependenciesOf<RegisterOptionsConfigurationAttribute>().GetTypes(f => f.WithAttribute<RegisterOptionsConfigurationAttribute>())
                                       );

                                       return services;
                                   }
                               }

                               """
                           )
                          .AddCompilationReferences(rootGenerator)
                          .AddCacheOptions(GetTempPath())
                          .Build()
                          .GenerateAsync();

        await Verify(result);
    }

    private static class StaticHelper
    {
        public static async Task<IServiceCollection> ExecuteStaticServiceCollectionMethod(Assembly? assembly, string className, string methodName)
        {
            if (assembly is null) return new ServiceCollection();

            var @class = assembly.GetTypes().FirstOrDefault(z => z.IsClass && z.Name == className)!;
            await Assert.That(@class).IsNotNull();

            var method = @class.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            await Assert.That(method).IsNotNull();

            return ( method!.Invoke(null, []) as IServiceCollection )!;
        }
    }
}
