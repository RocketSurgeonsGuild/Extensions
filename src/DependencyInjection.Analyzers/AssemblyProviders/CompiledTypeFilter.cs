using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal record CompiledTypeFilter
    (ClassFilter ClassFilter, ImmutableArray<ITypeFilterDescriptor> TypeFilterDescriptors) : ICompiledTypeFilter<INamedTypeSymbol>
{
    public bool Aborted { get; } = TypeFilterDescriptors.OfType<CompiledAbortTypeFilterDescriptor>().Any();

    public bool IsMatch(Compilation compilation, INamedTypeSymbol targetType)
    {
        if (Aborted || ( ClassFilter == ClassFilter.PublicOnly && targetType.DeclaredAccessibility != Accessibility.Public ))
            return false;

        if (TypeFilterDescriptors.Length == 0) return true;

        return TypeFilterDescriptors.All(GetFilterDescriptor);

        bool GetFilterDescriptor(ITypeFilterDescriptor filterDescriptor)
        {
            return filterDescriptor switch
                   {
                       AssignableToTypeFilterDescriptor { Type: var assignableToType } =>
                           Helpers.HasImplicitGenericConversion(compilation, assignableToType, targetType),
                       NotAssignableToTypeFilterDescriptor { Type: var notAssignableToType } =>
                           !Helpers.HasImplicitGenericConversion(compilation, notAssignableToType, targetType),
                       AssignableToAnyTypeFilterDescriptor { Types: var assignableToAnyTypes } =>
                           assignableToAnyTypes.Any(z => Helpers.HasImplicitGenericConversion(compilation, z, targetType)),
                       NotAssignableToAnyTypeFilterDescriptor { Types: var notAssignableToAnyTypes } =>
                           notAssignableToAnyTypes.All(z => !Helpers.HasImplicitGenericConversion(compilation, z, targetType)),
                       WithAttributeFilterDescriptor { Attribute: var attribute } =>
                           targetType.GetAttributes().Any(z => SymbolEqualityComparer.Default.Equals(z.AttributeClass, attribute)),
                       WithAnyAttributeFilterDescriptor { Attributes: var attributes } =>
                           handleWithAnyAttributeFilter(attributes, targetType),
                       WithoutAttributeFilterDescriptor { Attribute: var attribute } =>
                           targetType.GetAttributes().All(z => !SymbolEqualityComparer.Default.Equals(z.AttributeClass, attribute)),
                       WithAttributeStringFilterDescriptor { AttributeClassName: var attribute } =>
                           targetType.GetAttributes().Any(z => Helpers.GetFullMetadataName(z.AttributeClass) == attribute),
                       WithAnyAttributeStringFilterDescriptor { AttributeClassNames: var attributes } =>
                           targetType.GetAttributes().Join(attributes, z => Helpers.GetFullMetadataName(z.AttributeClass), z => z, (_, _) => true).Any(),
                       WithoutAttributeStringFilterDescriptor { AttributeClassName: var attribute } =>
                           targetType.GetAttributes().All(z => Helpers.GetFullMetadataName(z.AttributeClass) != attribute),
                       NamespaceFilterDescriptor { Filter: var filterName, Namespaces: var filterNamespaces } =>
                           handleNamespaceFilter(filterName, filterNamespaces, targetType),
                       NameFilterDescriptor { Include: var include, Filter: var filterName, Names: var filterNames } =>
                           handleNameFilter(include, filterName, filterNames, targetType),
                       TypeKindFilterDescriptor { Include: var include, TypeKinds: var typeKinds } =>
                           handleKindFilter(include, typeKinds, targetType),
                       TypeInfoFilterDescriptor { Include: var include, TypeInfos: var typeInfos } =>
                           handleInfoFilter(include, typeInfos, targetType),
                       _ => throw new NotSupportedException(filterDescriptor.GetType().FullName),
                   };
        }

        static bool handleWithAnyAttributeFilter(ImmutableHashSet<INamedTypeSymbol> attributes, INamedTypeSymbol targetType)
        {
            var targetAttributes = targetType.GetAttributes();
            foreach (var target in targetAttributes)
            {
                foreach (var attribute in attributes)
                {
                    if (attribute.Name == target.AttributeClass?.Name
                     && SymbolEqualityComparer.Default.Equals(attribute.ContainingNamespace, target.AttributeClass?.ContainingNamespace))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool handleNamespaceFilter(NamespaceFilter filterName, ImmutableHashSet<string> filterNamespaces, INamedTypeSymbol type)
        {
            var ns = type.ContainingNamespace.ToDisplayString();
            return filterName switch
                   {
                       NamespaceFilter.Exact => filterNamespaces.Contains(ns),
                       NamespaceFilter.In    => filterNamespaces.Any(n => ns.StartsWith(n)),
                       NamespaceFilter.NotIn => filterNamespaces.All(n => !ns.StartsWith(n)),
                       _                     => throw new NotImplementedException(),
                   };
        }

        static bool handleNameFilter(bool include, TextDirectionFilter filterName, ImmutableHashSet<string> filterNames, INamedTypeSymbol type)
        {
            return ( include, filterName ) switch
                   {
                       (true, TextDirectionFilter.Contains)    => filterNames.Any(name => type.Name.Contains(name)),
                       (false, TextDirectionFilter.Contains)   => !filterNames.Any(name => type.Name.Contains(name)),
                       (true, TextDirectionFilter.EndsWith)    => filterNames.Any(name => type.Name.EndsWith(name)),
                       (false, TextDirectionFilter.EndsWith)   => !filterNames.Any(name => type.Name.EndsWith(name)),
                       (true, TextDirectionFilter.StartsWith)  => filterNames.Any(name => type.Name.StartsWith(name)),
                       (false, TextDirectionFilter.StartsWith) => !filterNames.Any(name => type.Name.StartsWith(name)),
                       _                                       => throw new NotImplementedException(),
                   };
        }

        static bool handleKindFilter(bool include, ImmutableHashSet<TypeKind> typeKinds, INamedTypeSymbol type)
        {
            return include switch { true => typeKinds.Any(kind => type.TypeKind == kind), false => typeKinds.All(kind => type.TypeKind != kind) };
        }

        static bool handleInfoFilter(bool include, ImmutableHashSet<TypeInfoFilter> typeKinds, INamedTypeSymbol type)
        {
            return include switch
                   {
                       true  => typeKinds.Any(infoFilter => TypeInfoFilterFunc(infoFilter, type)),
                       false => typeKinds.All(infoFilter => !TypeInfoFilterFunc(infoFilter, type)),
                   };
        }

        static bool TypeInfoFilterFunc(TypeInfoFilter typeFilter, INamedTypeSymbol type)
        {
            return typeFilter switch
                   {
                       TypeInfoFilter.Abstract    => type.IsAbstract,
                       TypeInfoFilter.GenericType => type.IsGenericType,
//                TypeInfoFilter.GenericTypeDefinition => type is { IsGenericType: true, IsUnboundGenericType: true },
                       TypeInfoFilter.Sealed    => type.IsSealed,
                       TypeInfoFilter.Visible   => type.DeclaredAccessibility == Accessibility.Public,
                       TypeInfoFilter.ValueType => type.IsValueType,
//                TypeInfoFilter.Nested                => type.ContainingType is {},
                       TypeInfoFilter.Unknown => throw new NotSupportedException(typeFilter.ToString()),
                       _                      => throw new NotSupportedException(typeFilter.ToString()),
                   };
        }
    }
}
