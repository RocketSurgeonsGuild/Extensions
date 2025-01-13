using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Bogus;

using Microsoft.Extensions.DependencyInjection;

using Rocket.Surgery.DependencyInjection.Compiled;

using TestAssembly;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial record TestSource(string Name, string Source)
{
    public string GetTempDirectory(string? suffix = null) => (
            suffix is { }
                ? Path.Combine(ModuleInitializer.TempDirectory, GenerateFilenameSafeString(HashFilename(Source)), suffix)
                : Path.Combine(ModuleInitializer.TempDirectory, GenerateFilenameSafeString(HashFilename(Source)))
        )
       .Replace("\\", "/");

    public override string ToString() => GenerateFilenameSafeString(Name);

    private static string GenerateFilenameSafeString(string input) =>
        // Replace invalid filename characters with an underscore
        AssemblyScanningTests.TestData.ReplaceSpaces().Replace(MyRegex().Replace(input, ""), "");

    private static string HashFilename(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex MyRegex();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();
}

public partial class AssemblyScanningTests
{
    public static partial class TestData
    {
        public static IEnumerable<Func<TestSource>> GetTestData()
        {
            var assemblyRequests = GetAssemblyRequests().DistinctBy(z => z.TypeName).ToHashSet();
            var reflectionRequests = GetReflectionRequests().DistinctBy(z => z.TypeName).ToHashSet();
            var serviceDescriptorRequests = GetServiceDescriptorRequests().DistinctBy(z => z.TypeName).ToHashSet();

            var faker = new Faker { Random = new(1444) };

            foreach ((var Expression, var TypeName) in assemblyRequests)
            {
                yield return CreateTest(GetTypeName(Expression), [$"provider.GetAssemblies({Expression});"]);
            }

            foreach ((var Expression, var TypeName) in reflectionRequests)
            {
                yield return CreateTest(GetTypeName(Expression), [$"provider.GetTypes({Expression});"]);
            }

            foreach ((var Expression, var TypeName) in serviceDescriptorRequests)
            {
                yield return CreateTest(GetTypeName(Expression), [$"provider.Scan(services, {Expression});"]);
            }

            //            string[] expressions =
            //            [
            //                ..assemblyRequests.Select(item => $"provider.GetAssemblies({item.Expression});"),
            //                ..reflectionRequests.Select(item => $"provider.GetTypes({item.Expression});"),
            //                ..serviceDescriptorRequests.Select(item => $"provider.Scan(services, {item.Expression});")
            //            ];

            //            yield return CreateTest("all-together", expressions);

            static string GetTypeName(string expression)
            {
                var typeName = expression
                              .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                              .Select(z => z.Trim())
                              .Aggregate("", (x, y) => x + y)
                              .Trim();

                return ReplaceSpaces().Replace(typeName, "$1");
            }

            static Func<TestSource> CreateTest(string id, string[] expressions)
            {
                var source = $$$"""
                    using Rocket.Surgery.DependencyInjection;
                    using Rocket.Surgery.DependencyInjection.Compiled;
                    using Microsoft.Extensions.DependencyInjection;
                    using System.ComponentModel;
                    using System.Threading;
                    using System.Threading.Tasks;
                    using System;
                    using TestAssembly;

                    class Program
                    {
                        public static void Main(string[] args)
                        {
                            var services = new ServiceCollection();
                            var provider = typeof(Program).Assembly.GetCompiledTypeProvider();
                            {{{string.Join("\n", expressions)}}}
                        }
                    }
                    """;

                return () => new(id, source);
            }
        }

        [GeneratedRegex(@"(\s)\s+")]
        internal static partial Regex ReplaceSpaces();

        private static IEnumerable<(string Expression, string TypeName)> GetAssemblyRequests() =>
        [
            ToExpression<Action<IReflectionTypeSelector>>(z => z.FromAssemblies()),
            ToExpression<Action<IReflectionTypeSelector>>(z => z.FromAssemblies().NotFromAssemblyOf<ServiceRegistrationAttribute>()),
            ToExpression<Action<IReflectionTypeSelector>>(z => z.FromAssemblies().NotFromAssemblyOf<IService>()),
            ToExpression<Action<IReflectionTypeSelector>>(z => z.FromAssembly()),
            ToExpression<Action<IReflectionTypeSelector>>(z => z.FromAssemblyDependenciesOf<ServiceRegistrationAttribute>()),
            ToExpression<Action<IReflectionTypeSelector>>(z => z.FromAssemblyDependenciesOf<IService>()),
            //yield return TestMethod(z => z.IncludeSystemAssemblies().FromAssemblies());
        ];

        private static IEnumerable<(string Expression, string TypeName)> GetReflectionRequests() =>
        [
            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            //        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.InfoOf(TypeInfoFilter.Nested).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
                z => z
                    .FromAssemblies()
                    .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                    .GetTypes(
                         x => x
                             .NotAssignableTo<ICompiledTypeProvider>()
                             .InfoOf(TypeInfoFilter.Static)
                             .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                             .NotStartsWith("Polyfill")
                     )
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
                z => z
                    .FromAssemblies()
                    .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                    .GetTypes(
                         x => x
                             .NotAssignableTo<ICompiledTypeProvider>()
                             .NotInfoOf(TypeInfoFilter.Static)
                             .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
                             .NotStartsWith("Polyfill")
                     )
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            //        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.InfoOf(TypeInfoFilter.GenericTypeDefinition).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
                z => z
                    .FromAssemblies()
                    .FromAssemblyDependenciesOf<ServiceRegistrationAttribute>()
                    .GetTypes(x => x.WithAnyAttribute(typeof(ServiceRegistrationAttribute)))
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            //        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.NotInfoOf(TypeInfoFilter.Nested).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            #pragma warning disable CA2263

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            #pragma warning restore CA2263

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            //        yield return TestMethod(
            //            z => z
            //                .FromAssemblies()
            //                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
            //                .GetTypes(
            //                     x => x
            //                         .NotAssignableTo<ICompiledTypeProvider>()
            //                         .WithAttribute(typeof(RequiresUnreferencedCodeAttribute).FullName)
            //                         .NotInNamespaces("JetBrains.Annotations", "Polyfills", "System")
            //                         .NotStartsWith("Polyfill")
            //                 )
            //        );
            #pragma warning disable CA2263

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            #pragma warning restore CA2263

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
            //        yield return TestMethod(z => z.FromAssemblies().FromAssemblyOf<ConventionContext>().GetTypes(x => x.WithoutAttribute(typeof(JetBrains.Annotations.PublicAPIAttribute).FullName).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),

            ToExpression<Func<IReflectionTypeSelector, IEnumerable<Type>>>(
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
            ),
        ];

        private static IEnumerable<(string Expression, string TypeName)> GetServiceDescriptorRequests() =>
        [
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IServiceB>(), false)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>(), false)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>())
                    .As<IService>()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo(typeof(IGenericService<>)))
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo(typeof(IGenericService<>)))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>())
                    .As<IService>()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>().AssignableTo<IServiceB>())
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableToAny(typeof(IService), typeof(IServiceB)))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableToAny(typeof(IService), typeof(IServiceB)))
                    .AsSelf()
                    .AsImplementedInterfaces(z => z.AssignableTo<IServiceB>())
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableToAny(typeof(IOther)))
                    .AsSelf()
                    .AsImplementedInterfaces(z => z.AssignableTo(typeof(IGenericService<>)))
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableToAny(typeof(IValidator)))
                    .AsImplementedInterfaces(z => z.AssignableTo(typeof(IValidator<>)))
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableToAny(typeof(IService)))
                    .As<IService>()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableToAny(typeof(IOther)))
                    .AsSelf()
                    .As(typeof(IGenericService<>))
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableToAny(typeof(IOther)))
                    .As(typeof(IGenericService<>))
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
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
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.InNamespaces("TestAssembly"))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
            ToExpression<Action<IServiceDescriptorTypeSelector>>(
                z => z
                    .FromAssemblies()
                    .AddClasses(x => x.AssignableTo<IService>().AssignableTo<IServiceB>().EndsWith("Factory"))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            ),
        ];

        [GeneratedRegex(@"(\s)\s+")]
        private static partial Regex MyRegex();

        private static (string Expression, string TypeName) ToExpression<TSelector>(TSelector func, [CallerArgumentExpression(nameof(func))] string expression = null!)
        {
            var typeName = expression
                          .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                          .Select(z => MyRegex().Replace(z, "$1").Trim())
                          .Aggregate("", (x, y) => x + y)
                          .Trim();
            return (expression, typeName);
        }
    }
}
