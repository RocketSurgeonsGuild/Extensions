using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

// ReSharper disable UseCollectionExpression

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static partial class AssemblyProviderConfiguration
{
    public static (ImmutableList<AssemblyCollection.Item> AssemblyRequests, ImmutableList<ReflectionCollection.Item> ReflectionRequests,
        ImmutableList<ServiceDescriptorCollection.Item> ServiceDescriptorRequests) FromAssemblyAttributes(
            Compilation compilation
        )
    {
        var assemblySymbols = compilation
                             .References.Select(compilation.GetAssemblyOrModuleSymbol)
                             .Concat([compilation.Assembly])
                             .Select(
                                  symbol =>
                                  {
                                      if (symbol is IAssemblySymbol assemblySymbol)
                                          return assemblySymbol;
                                      if (symbol is IModuleSymbol moduleSymbol) return moduleSymbol.ContainingAssembly;
                                      // ReSharper disable once NullableWarningSuppressionIsUsed
                                      return null!;
                                  }
                              )
                             .Where(z => z is { })
                             .GroupBy(z => z.MetadataName, z => z, (s, symbols) => ( Key: s, Symbol: symbols.First() ))
                             .ToImmutableDictionary(z => z.Key, z => z.Symbol);

        var assemblyRequests = ImmutableList.CreateBuilder<AssemblyCollection.Item>();
        var reflectionRequests = ImmutableList.CreateBuilder<ReflectionCollection.Item>();
        var serviceDescriptorRequests = ImmutableList.CreateBuilder<ServiceDescriptorCollection.Item>();

        foreach (var assembly in assemblySymbols.Values)
        {
            var attributes = assembly.GetAttributes();
            foreach (var attribute in attributes)
            {
                if (attribute is not { AttributeClass.MetadataName : "AssemblyMetadataAttribute" }) continue;
                try
                {
                    switch (attribute)
                    {
                        case { ConstructorArguments: [{ Value: AssembliesKey }, { Value: string getAssembliesData }] }:
                            assemblyRequests.Add(GetAssembliesFromString(assemblySymbols, getAssembliesData));
                            break;
                        case { ConstructorArguments: [{ Value: ReflectionTypesKey }, { Value: string reflectionData }] }:
                            reflectionRequests.Add(GetReflectionFromString(compilation, assemblySymbols, reflectionData));
                            break;
                        case { ConstructorArguments: [{ Value: ServiceDescriptorTypesKey }, { Value: string serviceDescriptorData }] }:
                            serviceDescriptorRequests.Add(GetServiceDescriptorFromString(compilation, assemblySymbols, serviceDescriptorData));
                            break;
                    }
                }
                catch (InvalidOperationException)
                {
                    //?
                }
                catch (JsonException)
                {
                    //?
                }
            }
        }

        return ( assemblyRequests.ToImmutable(), reflectionRequests.ToImmutable(), serviceDescriptorRequests.ToImmutable() );
    }

    public static IEnumerable<AttributeListSyntax> ToAssemblyAttributes(
        ImmutableArray<AssemblyCollection.Item> assemblyRequests,
        ImmutableArray<ReflectionCollection.Item> reflectionTypes,
        ImmutableArray<ServiceDescriptorCollection.Item> serviceDescriptorTypes
    )
    {
        foreach (var request in assemblyRequests
                               .OrderBy(z => z.Location.FileName)
                               .ThenBy(z => z.Location.LineNumber)
                               .ThenBy(z => z.Location.ExpressionHash))
        {
            yield return Helpers.AddAssemblyAttribute(AssembliesKey, GetAssembliesToString(request));
        }

        foreach (var request in reflectionTypes
                               .OrderBy(z => z.Location.FileName)
                               .ThenBy(z => z.Location.LineNumber)
                               .ThenBy(z => z.Location.ExpressionHash))
        {
            yield return Helpers.AddAssemblyAttribute(ReflectionTypesKey, GetReflectionToString(request));
        }

        foreach (var request in serviceDescriptorTypes
                               .OrderBy(z => z.Location.FileName)
                               .ThenBy(z => z.Location.LineNumber)
                               .ThenBy(z => z.Location.ExpressionHash))
        {
            yield return Helpers.AddAssemblyAttribute(ServiceDescriptorTypesKey, GetServiceDescriptorToString(request));
        }
    }

    private const string AssembliesKey = "AssemblyProvider.GetAssemblies";
    private const string ReflectionTypesKey = "AssemblyProvider.ReflectionTypes";
    private const string ServiceDescriptorTypesKey = "AssemblyProvider.ServiceDescriptorTypes";

    private static AssemblyCollection.Item GetAssembliesFromString(ImmutableDictionary<string, IAssemblySymbol> assemblySymbols, string value)
    {
        var result = DecompressString(value);
        // ReSharper disable once NullableWarningSuppressionIsUsed
        var data = JsonSerializer.Deserialize(result, SourceGenerationContext.Default.AssemblyCollectionData)!;
        var assemblyFilter = LoadAssemblyFilter(data.Assembly, assemblySymbols);
        return new(data.Location, assemblyFilter);
    }

    private static string GetAssembliesToString(AssemblyCollection.Item item)
    {
        var data = new AssemblyCollectionData(item.Location, LoadAssemblyFilterData(item.AssemblyFilter));
        var result = JsonSerializer.SerializeToUtf8Bytes(data, SourceGenerationContext.Default.AssemblyCollectionData);
        return CompressString(result);
    }

    private static ReflectionCollection.Item GetReflectionFromString(
        Compilation compilation,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols,
        string value
    )
    {
        var result = DecompressString(value);
        // ReSharper disable once NullableWarningSuppressionIsUsed
        var data = JsonSerializer.Deserialize(result, SourceGenerationContext.Default.ReflectionCollectionData)!;
        var assemblyFilter = LoadAssemblyFilter(data.Assembly, assemblySymbols);
        var typeFilter = LoadTypeFilter(compilation, data.Type, assemblySymbols);
        return new(data.Location, assemblyFilter, typeFilter);
    }

    private static string GetReflectionToString(ReflectionCollection.Item item)
    {
        var data = new ReflectionCollectionData(
            item.Location,
            LoadAssemblyFilterData(item.AssemblyFilter),
            LoadTypeFilterData(item.AssemblyFilter, item.TypeFilter)
        );
        var result = JsonSerializer.SerializeToUtf8Bytes(data, SourceGenerationContext.Default.ReflectionCollectionData);
        return CompressString(result);
    }

    private static ServiceDescriptorCollection.Item GetServiceDescriptorFromString(
        Compilation compilation,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols,
        string value
    )
    {
        var result = DecompressString(value);
        // ReSharper disable once NullableWarningSuppressionIsUsed
        var data = JsonSerializer.Deserialize(result, SourceGenerationContext.Default.ServiceDescriptorCollectionData)!;
        var assemblyFilter = LoadAssemblyFilter(data.Assembly, assemblySymbols);
        var typeFilter = LoadTypeFilter(compilation, data.Type, assemblySymbols);
        // next up is services, and lifetime.
        var servicesFilter = LoadServiceDescriptorFilter(data.ServiceDescriptor);
        return new(data.Location, assemblyFilter, typeFilter, servicesFilter, data.Lifetime);
    }

    private static string GetServiceDescriptorToString(ServiceDescriptorCollection.Item item)
    {
        var data = new ServiceDescriptorCollectionData(
            item.Location,
            LoadAssemblyFilterData(item.AssemblyFilter),
            LoadTypeFilterData(item.AssemblyFilter, item.TypeFilter),
            LoadServiceDescriptorsData(item.AssemblyFilter, item.TypeFilter, item.ServicesTypeFilter),
            item.Lifetime
        );
        var result = JsonSerializer.SerializeToUtf8Bytes(data, SourceGenerationContext.Default.ServiceDescriptorCollectionData);
        return CompressString(result);
    }

    private static byte[] DecompressString(string base64String) => Convert.FromBase64String(base64String);

    private static string CompressString(byte[] bytes) => Convert.ToBase64String(bytes);

    private static AssemblyFilterData LoadAssemblyFilterData(CompiledAssemblyFilter filter)
    {
        return new(
            filter.AssemblyDescriptors.OfType<AllAssemblyDescriptor>().Any(),
            filter.AssemblyDescriptors.OfType<IncludeSystemAssembliesDescriptor>().Any(),
            filter.AssemblyDescriptors.OfType<AssemblyDescriptor>().Select(z => z.Assembly.MetadataName).OrderBy(z => z).ToImmutableArray(),
            filter.AssemblyDescriptors.OfType<NotAssemblyDescriptor>().Select(z => z.Assembly.MetadataName).OrderBy(z => z).ToImmutableArray(),
            filter.AssemblyDescriptors.OfType<AssemblyDependenciesDescriptor>().Select(z => z.Assembly.MetadataName).OrderBy(z => z).ToImmutableArray()
        );
    }

    private static TypeFilterData LoadTypeFilterData(CompiledAssemblyFilter assemblyFilter, CompiledTypeFilter typeFilter)
    {
        var assemblyData = LoadAssemblyFilterData(assemblyFilter);
        return new(
            assemblyData.AllAssembly,
            assemblyData.IncludeSystem,
            assemblyData.Assembly,
            assemblyData.NotAssembly,
            assemblyData.AssemblyDependencies,
            typeFilter.ClassFilter,
            typeFilter
               .TypeFilterDescriptors
               .OfType<NamespaceFilterDescriptor>()
               .Select(z => new NamespaceFilterData(z.Filter, z.Namespaces.OrderBy(z => z).ToImmutableArray()))
               .OrderBy(z => string.Join(",", z.Namespaces.OrderBy(static z => z)))
               .ThenBy(z => z.Filter)
               .Select(z => z with { Namespaces = z.Namespaces.OrderBy(z => z).ToImmutableArray() })
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors.OfType<NameFilterDescriptor>()
               .Select(z => new NameFilterData(z.Filter, z.Names.OrderBy(z => z).ToImmutableArray()))
               .OrderBy(z => string.Join(",", z.Names.OrderBy(static z => z)))
               .ThenBy(z => z.Filter)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors.OfType<TypeKindFilterDescriptor>()
               .Select(z => new TypeKindFilterData(z.Include, z.TypeKinds.OrderBy(z => z).ToImmutableArray()))
               .OrderBy(z => string.Join(",", z.TypeKinds.OrderBy(static z => z)))
               .ThenBy(z => z.Include)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors.OfType<TypeInfoFilterDescriptor>()
               .Select(z => new TypeInfoFilterData(z.Include, z.TypeInfos.OrderBy(z => z).ToImmutableArray()))
               .OrderBy(z => string.Join(",", z.TypeInfos.OrderBy(static z => z)))
               .ThenBy(z => z.Include)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors
               .Select(
                    f => f switch
                         {
                             WithAttributeFilterDescriptor descriptor => new(
                                 true,
                                 descriptor.Attribute.ContainingAssembly.MetadataName,
                                 Helpers.GetFullMetadataName(descriptor.Attribute),
                                 descriptor.Attribute.IsUnboundGenericType
                             ),
                             WithoutAttributeFilterDescriptor descriptor => new WithAttributeData(
                                 false,
                                 descriptor.Attribute.ContainingAssembly.MetadataName,
                                 Helpers.GetFullMetadataName(descriptor.Attribute),
                                 descriptor.Attribute.IsUnboundGenericType
                             ),
                             _ => null!,
                         }
                )
               .Where(z => z is { })
               .OrderBy(z => z.Assembly)
               .ThenBy(z => z.Attribute)
               .ThenBy(z => z.Include)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors
               .Select(
                    f => f switch
                         {
                             WithAttributeStringFilterDescriptor descriptor    => new(true, descriptor.AttributeClassName),
                             WithoutAttributeStringFilterDescriptor descriptor => new WithAttributeStringData(false, descriptor.AttributeClassName),
                             _                                                 => null!,
                         }
                )
               .Where(z => z is { })
               .OrderBy(z => z.Attribute)
               .ThenBy(z => z.Include)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors
               .SelectMany(
                    f => f switch
                         {
                             WithAnyAttributeFilterDescriptor descriptor =>
                                 descriptor.Attributes
                                           .Select(
                                                attribute => new WithAttributeData(
                                                    true,
                                                    attribute.ContainingAssembly.MetadataName,
                                                    Helpers.GetFullMetadataName(attribute),
                                                    attribute.IsUnboundGenericType
                                                )
                                            ),
                             _ => [],
                         }
                )
               .Where(z => z is { })
               .OrderBy(z => z.Assembly)
               .ThenBy(z => z.Attribute)
               .ThenBy(z => z.Include)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors
               .SelectMany(
                    f =>
                        f switch
                        {
                            WithAnyAttributeStringFilterDescriptor descriptor => descriptor.AttributeClassNames.Select(
                                z => new WithAttributeStringData(true, z)
                            ),
                            _ => [],
                        }
                )
               .Where(z => z is { })
               .OrderBy(z => z.Attribute)
               .ThenBy(z => z.Include)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors
               .Select(
                    f => f switch
                         {
                             AssignableToTypeFilterDescriptor descriptor => new(
                                 true,
                                 descriptor.Type.ContainingAssembly.MetadataName,
                                 Helpers.GetFullMetadataName(descriptor.Type),
                                 descriptor.Type.IsUnboundGenericType
                             ),
                             NotAssignableToTypeFilterDescriptor descriptor => new AssignableToTypeData(
                                 false,
                                 descriptor.Type.ContainingAssembly.MetadataName,
                                 Helpers.GetFullMetadataName(descriptor.Type),
                                 descriptor.Type.IsUnboundGenericType
                             ),
                             _ => null!,
                         }
                )
               .Where(z => z is { })
               .OrderBy(z => z.Assembly)
               .ThenBy(z => z.Type)
               .ThenBy(z => z.Include)
               .ToImmutableArray(),
            typeFilter
               .TypeFilterDescriptors
               .Select(
                    f => f switch
                         {
                             AssignableToAnyTypeFilterDescriptor descriptor => new(
                                 true,
                                 descriptor
                                    .Types.Select(
                                         z => new AnyTypeData(z.ContainingAssembly.MetadataName, Helpers.GetFullMetadataName(z), z.IsUnboundGenericType)
                                     )
                                    .OrderBy(z => z.Assembly)
                                    .ThenBy(z => z.Type)
                                    .ToImmutableArray()
                             ),
                             NotAssignableToAnyTypeFilterDescriptor descriptor => new AssignableToAnyTypeData(
                                 false,
                                 descriptor
                                    .Types
                                    .Select(z => new AnyTypeData(z.ContainingAssembly.MetadataName, Helpers.GetFullMetadataName(z), z.IsUnboundGenericType))
                                    .OrderBy(z => z.Assembly)
                                    .ThenBy(z => z.Type)
                                    .ToImmutableArray()
                             ),
                             _ => null!,
                         }
                )
               .Where(z => z is { })
               .OrderBy(z => string.Join(",", z.Types))
               .ThenBy(z => z.Include)
               .ToImmutableArray()
        );
    }

    private static CompiledAssemblyFilter LoadAssemblyFilter(AssemblyFilterData data, ImmutableDictionary<string, IAssemblySymbol> assemblySymbols)
    {
        var descriptors = ImmutableArray.CreateBuilder<IAssemblyDescriptor>();
        if (data.AllAssembly) descriptors.Add(new AllAssemblyDescriptor());

        if (data.IncludeSystem) descriptors.Add(new IncludeSystemAssembliesDescriptor());

        foreach (var item in data.Assembly)
        {
            if (assemblySymbols.TryGetValue(item, out var assembly)) descriptors.Add(new AssemblyDescriptor(assembly));
        }

        foreach (var item in data.NotAssembly)
        {
            if (assemblySymbols.TryGetValue(item, out var assembly)) descriptors.Add(new NotAssemblyDescriptor(assembly));
        }

        foreach (var item in data.AssemblyDependencies)
        {
            if (assemblySymbols.TryGetValue(item, out var assembly)) descriptors.Add(new AssemblyDependenciesDescriptor(assembly));
        }

        return new(descriptors.ToImmutable());
    }

    private static CompiledTypeFilter LoadTypeFilter(
        Compilation compilation,
        TypeFilterData data,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols
    )
    {
        var descriptors = ImmutableArray.CreateBuilder<ITypeFilterDescriptor>();
        foreach (var item in data.NamespaceFilters)
        {
            descriptors.Add(new NamespaceFilterDescriptor(item.Filter, item.Namespaces.ToImmutableHashSet()));
        }

        foreach (var item in data.NameFilters)
        {
            descriptors.Add(new NameFilterDescriptor(item.Filter, item.Names.ToImmutableHashSet()));
        }

        foreach (var item in data.TypeKindFilters)
        {
            descriptors.Add(new TypeKindFilterDescriptor(item.Include, item.TypeKinds.ToImmutableHashSet()));
        }

        foreach (var item in data.TypeInfoFilters)
        {
            descriptors.Add(new TypeInfoFilterDescriptor(item.Include, item.TypeInfos.ToImmutableHashSet()));
        }

        foreach (var item in data.WithAttributeFilters)
        {
            if (findType(assemblySymbols, compilation, item.Assembly, item.Attribute) is not { } type) continue;
            if (item.UnboundGenericType) type = type.ConstructUnboundGenericType();
            descriptors.Add(item.Include ? new WithAttributeFilterDescriptor(type) : new WithoutAttributeFilterDescriptor(type));
        }

        foreach (var item in data.WithAttributeStringFilters)
        {
            descriptors.Add(
                item.Include ? new WithAttributeStringFilterDescriptor(item.Attribute) : new WithoutAttributeStringFilterDescriptor(item.Attribute)
            );
        }

        var withAnyAttributeFilter = new List<INamedTypeSymbol>();
        foreach (var item in data.WithAnyAttributeFilters)
        {
            if (findType(assemblySymbols, compilation, item.Assembly, item.Attribute) is not { } type) continue;
            if (item.UnboundGenericType) type = type.ConstructUnboundGenericType();
            withAnyAttributeFilter.Add(type);
        }

        if (withAnyAttributeFilter.Any())
        {
            descriptors.Add(new WithAnyAttributeFilterDescriptor(withAnyAttributeFilter.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)));
        }

        var withAnyAttributeStringFilter = new List<string>();
        foreach (var item in data.WithAnyAttributeStringFilters)
        {
            withAnyAttributeStringFilter.Add(item.Attribute);
        }

        if (withAnyAttributeStringFilter.Any())
        {
            descriptors.Add(new WithAnyAttributeStringFilterDescriptor(withAnyAttributeStringFilter.ToImmutableHashSet()));
        }

        foreach (var item in data.AssignableToTypeFilters)
        {
            if (findType(assemblySymbols, compilation, item.Assembly, item.Type) is not { } type) continue;
            if (item.UnboundGenericType) type = type.ConstructUnboundGenericType();
            descriptors.Add(item.Include ? new AssignableToTypeFilterDescriptor(type) : new NotAssignableToTypeFilterDescriptor(type));
        }

        foreach (var item in data.AssignableToAnyTypeFilters)
        {
            var filters = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            foreach (var typeData in item.Types)
            {
                if (findType(assemblySymbols, compilation, typeData.Assembly, typeData.Type) is not { } type) continue;
                if (typeData.UnboundGenericType) type = type.ConstructUnboundGenericType();
                filters.Add(type);
            }

            descriptors.Add(
                item.Include
                    ? new AssignableToAnyTypeFilterDescriptor(filters.ToImmutable())
                    : new NotAssignableToAnyTypeFilterDescriptor(filters.ToImmutable())
            );
        }

        return new(data.Filter, descriptors.ToImmutable());

        static INamedTypeSymbol? findType(
            ImmutableDictionary<string, IAssemblySymbol> assemblySymbols,
            Compilation compilation,
            string assemblyName,
            string typeName
        )
        {
            if (CompiledAssemblyFilter._coreAssemblies.Contains(assemblyName)) return compilation.GetTypeByMetadataName(typeName);

            return !assemblySymbols.TryGetValue(assemblyName, out var assembly)
             || FindTypeVisitor.FindType(compilation, assembly, typeName) is not { } type
                    ? compilation.GetTypeByMetadataName(typeName)
                    : type;
        }
    }

    private static ServiceDescriptorFilterData LoadServiceDescriptorsData(
        CompiledAssemblyFilter assemblyFilter,
        CompiledTypeFilter typeFilter,
        CompiledServiceTypeDescriptors serviceTypeDescriptors
    )
    {
        var assemblyData = LoadAssemblyFilterData(assemblyFilter);
        var typeData = LoadTypeFilterData(assemblyFilter, typeFilter);
        var serviceDescriptors = serviceTypeDescriptors.ServiceTypeDescriptors.Select(
            z => z switch
                 {
                     CompiledServiceTypeDescriptor              => "c",
                     ImplementedInterfacesServiceTypeDescriptor => "i",
                     MatchingInterfaceServiceTypeDescriptor     => "m",
                     SelfServiceTypeDescriptor                  => "s",
                     UsingAttributeServiceTypeDescriptor        => "u",
                     _                                          => throw new ArgumentOutOfRangeException(nameof(z)),
                 }
        );
        return new(
            assemblyData.AllAssembly,
            assemblyData.IncludeSystem,
            assemblyData.Assembly,
            assemblyData.NotAssembly,
            assemblyData.AssemblyDependencies,
            typeData.Filter,
            typeData.NamespaceFilters,
            typeData.NameFilters,
            typeData.TypeKindFilters,
            typeData.TypeInfoFilters,
            typeData.WithAttributeFilters,
            typeData.WithAttributeStringFilters,
            typeData.WithAnyAttributeFilters,
            typeData.WithAnyAttributeStringFilters,
            typeData.AssignableToTypeFilters,
            typeData.AssignableToAnyTypeFilters,
            serviceDescriptors.ToImmutableArray(),
            serviceTypeDescriptors.Lifetime
        );
    }

    private static CompiledServiceTypeDescriptors LoadServiceDescriptorFilter(ServiceDescriptorFilterData data)
    {
        var descriptors = ImmutableArray.CreateBuilder<IServiceTypeDescriptor>();
        foreach (var item in data.ServiceTypeDescriptors)
        {
            descriptors.Add(
                item switch
                {
                    "c" => new CompiledServiceTypeDescriptor(),
                    "i" => new ImplementedInterfacesServiceTypeDescriptor(),
                    "m" => new MatchingInterfaceServiceTypeDescriptor(),
                    "s" => new SelfServiceTypeDescriptor(),
                    "u" => new UsingAttributeServiceTypeDescriptor(),
                    _   => throw new ArgumentOutOfRangeException(nameof(data)),
                }
            );
        }

        return new(descriptors.ToImmutable(), data.Lifetime);
    }

    [JsonSourceGenerationOptions]
    [JsonSerializable(typeof(AssemblyCollectionData))]
    [JsonSerializable(typeof(ReflectionCollectionData))]
    [JsonSerializable(typeof(ServiceDescriptorCollectionData))]
    [JsonSerializable(typeof(AssemblyFilterData))]
    [JsonSerializable(typeof(TypeFilterData))]
    [JsonSerializable(typeof(NamespaceFilterData))]
    [JsonSerializable(typeof(NameFilterData))]
    [JsonSerializable(typeof(TypeKindFilterData))]
    [JsonSerializable(typeof(TypeInfoFilterData))]
    [JsonSerializable(typeof(WithAttributeData))]
    [JsonSerializable(typeof(WithAttributeStringData))]
    [JsonSerializable(typeof(AssignableToTypeData))]
    [JsonSerializable(typeof(AssignableToAnyTypeData))]
    private partial class SourceGenerationContext : JsonSerializerContext;

    private record AssemblyCollectionData
    (
        [property: JsonPropertyName("l")]
        SourceLocation Location,
        [property: JsonPropertyName("a")]
        AssemblyFilterData Assembly
    );

    private record ReflectionCollectionData
    (
        [property: JsonPropertyName("l")]
        SourceLocation Location,
        [property: JsonPropertyName("a")]
        AssemblyFilterData Assembly,
        [property: JsonPropertyName("t")]
        TypeFilterData Type
    );

    private record ServiceDescriptorCollectionData
    (
        [property: JsonPropertyName("l")]
        SourceLocation Location,
        [property: JsonPropertyName("a")]
        AssemblyFilterData Assembly,
        [property: JsonPropertyName("t")]
        TypeFilterData Type,
        [property: JsonPropertyName("s")]
        ServiceDescriptorFilterData ServiceDescriptor,
        [property: JsonPropertyName("z")]
        int Lifetime
    );

    private record AssemblyFilterData
    (
        [property: JsonPropertyName("a")]
        bool AllAssembly,
        [property: JsonPropertyName("i")]
        bool IncludeSystem,
        [property: JsonPropertyName("m")]
        ImmutableArray<string> Assembly,
        [property: JsonPropertyName("na")]
        ImmutableArray<string> NotAssembly,
        [property: JsonPropertyName("d")]
        ImmutableArray<string> AssemblyDependencies
    );

    private record TypeFilterData
    (
        bool AllAssembly,
        bool IncludeSystem,
        ImmutableArray<string> Assembly,
        ImmutableArray<string> NotAssembly,
        ImmutableArray<string> AssemblyDependencies,
        [property: JsonPropertyName("f")]
        ClassFilter Filter,
        [property: JsonPropertyName("nsf")]
        ImmutableArray<NamespaceFilterData> NamespaceFilters,
        [property: JsonPropertyName("nf")]
        ImmutableArray<NameFilterData> NameFilters,
        [property: JsonPropertyName("tk")]
        ImmutableArray<TypeKindFilterData> TypeKindFilters,
        [property: JsonPropertyName("ti")]
        ImmutableArray<TypeInfoFilterData> TypeInfoFilters,
        [property: JsonPropertyName("w")]
        ImmutableArray<WithAttributeData> WithAttributeFilters,
        [property: JsonPropertyName("s")]
        ImmutableArray<WithAttributeStringData> WithAttributeStringFilters,
        [property: JsonPropertyName("wa")]
        ImmutableArray<WithAttributeData> WithAnyAttributeFilters,
        [property: JsonPropertyName("sa")]
        ImmutableArray<WithAttributeStringData> WithAnyAttributeStringFilters,
        [property: JsonPropertyName("at")]
        ImmutableArray<AssignableToTypeData> AssignableToTypeFilters,
        [property: JsonPropertyName("ta")]
        ImmutableArray<AssignableToAnyTypeData> AssignableToAnyTypeFilters
    ) : AssemblyFilterData(AllAssembly, IncludeSystem, Assembly, NotAssembly, AssemblyDependencies);

    private record ServiceDescriptorFilterData
    (
        bool AllAssembly,
        bool IncludeSystem,
        ImmutableArray<string> Assembly,
        ImmutableArray<string> NotAssembly,
        ImmutableArray<string> AssemblyDependencies,
        ClassFilter Filter,
        ImmutableArray<NamespaceFilterData> NamespaceFilters,
        ImmutableArray<NameFilterData> NameFilters,
        ImmutableArray<TypeKindFilterData> TypeKindFilters,
        ImmutableArray<TypeInfoFilterData> TypeInfoFilters,
        ImmutableArray<WithAttributeData> WithAttributeFilters,
        ImmutableArray<WithAttributeStringData> WithAttributeStringFilters,
        ImmutableArray<WithAttributeData> WithAnyAttributeFilters,
        ImmutableArray<WithAttributeStringData> WithAnyAttributeStringFilters,
        ImmutableArray<AssignableToTypeData> AssignableToTypeFilters,
        ImmutableArray<AssignableToAnyTypeData> AssignableToAnyTypeFilters,
        ImmutableArray<string> ServiceTypeDescriptors,
        int Lifetime
    ) : TypeFilterData(
        AllAssembly,
        IncludeSystem,
        Assembly,
        NotAssembly,
        AssemblyDependencies,
        Filter,
        NamespaceFilters,
        NameFilters,
        TypeKindFilters,
        TypeInfoFilters,
        WithAttributeFilters,
        WithAttributeStringFilters,
        WithAnyAttributeFilters,
        WithAnyAttributeStringFilters,
        AssignableToTypeFilters,
        AssignableToAnyTypeFilters
    );

    internal record WithAttributeData
    (
        [property: JsonPropertyName("i")]
        bool Include,
        [property: JsonPropertyName("a")]
        string Assembly,
        [property: JsonPropertyName("b")]
        string Attribute,
        [property: JsonPropertyName("u")]
        bool UnboundGenericType);

    internal record WithAttributeStringData
    (
        [property: JsonPropertyName("i")]
        bool Include,
        [property: JsonPropertyName("b")]
        string Attribute);

    internal record AssignableToTypeData
    (
        [property: JsonPropertyName("i")]
        bool Include,
        [property: JsonPropertyName("a")]
        string Assembly,
        [property: JsonPropertyName("t")]
        string Type,
        [property: JsonPropertyName("u")]
        bool UnboundGenericType);

    internal record AssignableToAnyTypeData
    (
        [property: JsonPropertyName("i")]
        bool Include,
        [property: JsonPropertyName("t")]
        ImmutableArray<AnyTypeData> Types);

    internal record AnyTypeData
    (
        [property: JsonPropertyName("a")]
        string Assembly,
        [property: JsonPropertyName("t")]
        string Type,
        [property: JsonPropertyName("u")]
        bool UnboundGenericType);


    internal record NamespaceFilterData
    (
        [property: JsonPropertyName("f")]
        NamespaceFilter Filter,
        [property: JsonPropertyName("n")]
        ImmutableArray<string> Namespaces);

    internal record NameFilterData
    (
        [property: JsonPropertyName("f")]
        TextDirectionFilter Filter,
        [property: JsonPropertyName("n")]
        ImmutableArray<string> Names);

    internal record TypeKindFilterData
    (
        [property: JsonPropertyName("f")]
        bool Include,
        [property: JsonPropertyName("t")]
        ImmutableArray<TypeKind> TypeKinds);

    internal record TypeInfoFilterData
    (
        [property: JsonPropertyName("f")]
        bool Include,
        [property: JsonPropertyName("t")]
        ImmutableArray<TypeInfoFilter> TypeInfos);
}
