using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public static class GetTypesTestsData
{
    // IEnumerable<Func<BindDelegate>>
    public static IEnumerable<Func<GetTypesItem>> GetTypesData()
    {
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
                         .InfoOf(TypeInfoFilter.Static)
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
                         .NotInfoOf(TypeInfoFilter.Static)
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

        static Func<GetTypesItem> TestMethod(
            Func<IReflectionTypeSelector, IEnumerable<Type>> func,
            [CallerArgumentExpression(nameof(func))]
            string argument = null!
        )
        {
            // TODO: REmove this once tests pass
            // .Replace("\r", "").Replace("\n", "")
            var typeName = argument[( argument.LastIndexOf("=> x", StringComparison.Ordinal) + 5 )..^1]
                          .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                          .Select(z => z.Trim())
                          .Aggregate("", (x, y) => x + y)
                          .Trim();
            return () => new(typeName, argument, func);
        }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public record GetTypesItem(string Name, string Expression, Func<IReflectionTypeSelector, IEnumerable<Type>> Selector)
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => ToString();

        public override string ToString() => Name;
    }
}

public static class GetAssembliesTestsData
{
    // IEnumerable<Func<BindDelegate>>
    public static IEnumerable<Func<GetAssembliesItem>> GetAssembliesData()
    {
        yield return TestMethod(z => z.FromAssemblies());
        yield return TestMethod(z => z.FromAssemblies().NotFromAssemblyOf<ServiceRegistrationAttribute>());
        yield return TestMethod(z => z.FromAssembly());
        yield return TestMethod(z => z.FromAssemblyDependenciesOf<ServiceRegistrationAttribute>());
        yield return TestMethod(z => z.IncludeSystemAssemblies().FromAssemblies());
        static Func<GetAssembliesItem> TestMethod(
            Action<IReflectionTypeSelector> func,
            [CallerArgumentExpression(nameof(func))]
            string argument = null!
        )
        {
            // TODO: REmove this once tests pass
            // .Replace("\r", "").Replace("\n", "")
            var typeName = argument[( argument.LastIndexOf("=> x", StringComparison.Ordinal) + 5 )..^1]
                          .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                          .Select(z => z.Trim())
                          .Aggregate("", (x, y) => x + y)
                          .Trim();
            return () => new(typeName, argument, func);
        }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public record GetAssembliesItem(string Name, string Expression, Action<IReflectionTypeSelector> Selector)
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => ToString();

        public override string ToString() => Name;
    }
}
