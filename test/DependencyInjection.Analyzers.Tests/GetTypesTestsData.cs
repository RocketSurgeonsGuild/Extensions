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
        // ReSharper disable RedundantNameQualifier
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().NotKindOf(TypeKindFilter.Delegate, TypeKindFilter.Class))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().NotKindOf(TypeKindFilter.Delegate, TypeKindFilter.Class, TypeKindFilter.Enum))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().NotKindOf(TypeKindFilter.Delegate).NotKindOf(TypeKindFilter.Interface))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InfoOf(TypeInfoFilter.Abstract).NotInNamespaces("JetBrains.Annotations"))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InfoOf(TypeInfoFilter.Visible).NotInNamespaces("JetBrains.Annotations"))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InfoOf(TypeInfoFilter.ValueType).NotInNamespaces("JetBrains.Annotations"))
        );
//        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.InfoOf(TypeInfoFilter.Nested).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InfoOf(TypeInfoFilter.Sealed).NotInNamespaces("JetBrains.Annotations"))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InfoOf(TypeInfoFilter.GenericType).NotInNamespaces("JetBrains.Annotations"))
        );
//        yield return TestMethod(z => z.FromAssemblies().GetTypes(x => x.InfoOf(TypeInfoFilter.GenericTypeDefinition).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().NotInfoOf(TypeInfoFilter.Abstract).NotInNamespaces("JetBrains.Annotations"))
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
                         .NotInNamespaces("JetBrains.Annotations")
                 )
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
                         .NotInNamespaces("JetBrains.Annotations")
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
                         .NotInNamespaces("JetBrains.Annotations")
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
                         .NotInNamespaces("JetBrains.Annotations")
                 )
        );
        #pragma warning disable CA2263
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().WithAttribute(typeof(EditorBrowsableAttribute)))
        );
        #pragma warning restore CA2263
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().WithAttribute<EditorBrowsableAttribute>())
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
                         .NotInNamespaces("JetBrains.Annotations")
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
                         .NotInNamespaces("JetBrains.Annotations")
                 )
        );
//        yield return TestMethod(z => z.FromAssemblies().FromAssemblyOf<ConventionContext>().GetTypes(x => x.WithoutAttribute(typeof(JetBrains.Annotations.PublicAPIAttribute).FullName).NotAssignableTo<Attribute>().NotInNamespaces("JetBrains.Annotations")));
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InNamespaceOf(typeof(ServiceCollectionServiceExtensions)))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InNamespaceOf<IServiceCollection>())
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(x => x.NotAssignableTo<ICompiledTypeProvider>().InNamespaces("Microsoft.Extensions.DependencyInjection"))
        );
        yield return TestMethod(
            z => z
                .FromAssemblies()
                .NotFromAssemblyOf<ServiceRegistrationAttribute>()
                .GetTypes(
                     true,
                     x => x.NotAssignableTo<ICompiledTypeProvider>().NotInNamespaces("JetBrains.Annotations")
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

    public record GetTypesItem(string Name, string Expression, Func<IReflectionTypeSelector, IEnumerable<Type>> Selector)
    {
        public override string ToString() => Name;
    }
}
