using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.DependencyInjection.Compiled;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
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
                      typeof(IServiceCollection).Assembly,
                      typeof(ServiceCollection).Assembly,
                      typeof(ServiceRegistrationAttribute).Assembly,
                      typeof(EditorBrowsableAttribute).Assembly,
                      typeof(Attribute).Assembly
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

public static class GetTypesTestsData
{
    public static IEnumerable<object[]> GetTypesData()
    {
        // ReSharper disable RedundantNameQualifier
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotKindOf(TypeKindFilter.Delegate, TypeKindFilter.Class)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotKindOf(TypeKindFilter.Delegate, TypeKindFilter.Class, TypeKindFilter.Enum)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotKindOf(TypeKindFilter.Delegate)
                         .NotKindOf(TypeKindFilter.Interface)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .InfoOf(TypeInfoFilter.Abstract)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .InfoOf(TypeInfoFilter.Visible)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .InfoOf(TypeInfoFilter.ValueType)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
//        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.InfoOf(TypeInfoFilter.Nested).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .InfoOf(TypeInfoFilter.Sealed)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .InfoOf(TypeInfoFilter.GenericType)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
//        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.InfoOf(TypeInfoFilter.GenericTypeDefinition).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotInfoOf(TypeInfoFilter.Abstract)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotInfoOf(TypeInfoFilter.Visible)
                         .NotAssignableTo<Attribute>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .FromAssemblyDependenciesOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.WithAnyAttribute(typeof(ServiceRegistrationAttribute)))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotInfoOf(TypeInfoFilter.ValueType)
                         .NotAssignableTo<Attribute>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
//        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.NotInfoOf(TypeInfoFilter.Nested).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotInfoOf(TypeInfoFilter.Sealed)
                         .NotAssignableTo<Attribute>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotInfoOf(TypeInfoFilter.GenericType)
                         .NotAssignableTo<Attribute>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        #pragma warning disable CA2263
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .WithAttribute(typeof(EditorBrowsableAttribute))
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        #pragma warning restore CA2263
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .WithAttribute<EditorBrowsableAttribute>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .WithAttribute(typeof(System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute).FullName)
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        #pragma warning disable CA2263
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .WithoutAttribute(typeof(EditorBrowsableAttribute))
                         .NotAssignableTo(typeof(Attribute))
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        #pragma warning restore CA2263
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .WithoutAttribute<EditorBrowsableAttribute>()
                         .NotAssignableTo<Attribute>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
//        yield return TestMethod(z => z.FromAssemblies().FromAssemblyOf<ConventionContext>().GetTypes(x => x.WithoutAttribute(typeof(JetBrains.Annotations.PublicAPIAttribute).FullName).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .InNamespaceOf(typeof(ServiceCollectionServiceExtensions))
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .InNamespaceOf<IServiceCollection>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .InNamespaces("Microsoft.Extensions.DependencyInjection")
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     true,
                     x => x
                         .NotAssignableTo<ICompiledTypeProvider>()
                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                         .NotStartsWith("Polyfill")
                 )
        );

        static object[] TestMethod(
            Func<IReflectionTypeSelector, IEnumerable<Type>> func,
            [CallerArgumentExpression(nameof(func))]
            string argument = null!
        )
        {
            // TODO: REmove this once tests pass
            // .Replace("\r", "").Replace("\n", "")
            argument = argument
                      .Replace(".NotInNamespaces(\"JetBrains.Annotations\", \"Polyfills\", \"System\")", "")
                      .Replace(".NotInNamespaces(\"JetBrains.Annotations\")", "")
                      .Replace(".NotStartsWith(\"Polyfills\")", "");
            var typeName = argument[( argument.LastIndexOf("=> x") + 5 )..^1]
                          .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(z => z.Trim())
                          .Aggregate("", (x, y) => x + y)
                          .Trim();
            return [new GetTypesItem(typeName, argument, func)];
        }
    }

    public record GetTypesItem(string Name, string Expression, Func<IReflectionTypeSelector, IEnumerable<Type>> Selector)
    {
        public override string ToString() => Name;
    }
}
