using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal partial class AssemblyProviderConfiguration
(
    SourceProductionContext context,
    Compilation compilation,
    AnalyzerConfigOptionsProvider options,
    FrozenDictionary<string, CompiledAssemblyProviderData> generatedJson,
    FrozenSet<string> skip,
    FrozenDictionary<string, SavedSourceLocation> partial
)
{
#pragma warning disable RS1035
    private readonly Lazy<string?> _cacheDirectory = new(
        () =>
        {
            var directory = options.GlobalOptions.TryGetValue("build_property.IntermediateOutputPath", out var intermediateOutputPath)
                ? intermediateOutputPath
                : null;
            if (!Path.IsPathRooted(directory) && options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDirectory))
                directory = Path.Combine(projectDirectory, directory);
            if (directory is null) return null;
            var cacheDirectory = Path.Combine(directory, "GeneratedAssemblyProvider");
            if (!Directory.Exists(cacheDirectory)) Directory.CreateDirectory(cacheDirectory);

            return cacheDirectory;
        }
    );
#pragma warning restore RS1035

    private static string GetCacheFileHash(SourceLocation location, IAssemblySymbol assemblySymbol)
    {
        using var hasher = MD5.Create();
        hasher.ComputeHash(Encoding.UTF8.GetBytes(location.FileName));
        hasher.ComputeHash(Encoding.UTF8.GetBytes(location.ExpressionHash));
        hasher.ComputeHash(Encoding.UTF8.GetBytes(location.LineNumber.ToString()));
        hasher.ComputeHash(Encoding.UTF8.GetBytes(assemblySymbol.MetadataName));
        return hasher.Hash.Aggregate("", (s, b) => s + b.ToString("x2"));
    }

#pragma warning disable RS1035
    private ResolvedSourceLocation? CacheSourceLocation(SourceLocation location, IAssemblySymbol assemblySymbol, SourceLocationKind kind, Func<ResolvedSourceLocation?> factory)
    {
        var cacheKey = $"{kind}-{GetCacheFileHash(location, assemblySymbol)}{Constants.PartialExtension}";
        if (partial.TryGetValue(cacheKey, out var text))
        {
            return new(location, text.Expression, [..text.PrivateAssemblies]);
        }

        var source = factory();
        if (source is {} && _cacheDirectory.Value is { } && !File.Exists(Path.Combine(_cacheDirectory.Value, cacheKey)))
            File.WriteAllText(Path.Combine(_cacheDirectory.Value, cacheKey), JsonSerializer.Serialize(new(kind, source.Expression, [..source.PrivateAssemblies]), JsonSourceGenerationContext.Default.SavedSourceLocation));
        return source;
    }
#pragma warning restore RS1035

    public (
        ImmutableList<AssemblyCollection.Item> InternalAssemblyRequests,
        ImmutableList<ReflectionCollection.Item> InternalReflectionRequests,
        ImmutableList<ResolvedSourceLocation> ReflectionSources,
        ImmutableList<ServiceDescriptorCollection.Item> InternalServiceDescriptorRequests,
        ImmutableList<ResolvedSourceLocation> ServiceDescriptorSources
        ) FromAssemblyAttributes(
            ImmutableDictionary<string, IAssemblySymbol> assemblySymbols,
            ImmutableList<ReflectionCollection.Item> reflectionRequests,
            ImmutableList<ServiceDescriptorCollection.Item> serviceDescriptorRequests,
            HashSet<Diagnostic> globalDiagnostics
        )
   {
        var reflectionSources = ImmutableList.CreateBuilder<ResolvedSourceLocation>();
        var serviceDescriptorSources = ImmutableList.CreateBuilder<ResolvedSourceLocation>();

        var internalAssemblyRequestsBuilder = ImmutableList.CreateBuilder<AssemblyCollection.Item>();
        var internalReflectionRequestsBuilder = ImmutableList.CreateBuilder<ReflectionCollection.Item>();
        var internalServiceDescriptorRequestsBuilder = ImmutableList.CreateBuilder<ServiceDescriptorCollection.Item>();
        foreach (var assembly in assemblySymbols.Values.Except([compilation.Assembly]).OrderBy(z => z.MetadataName))
        {
            try
            {
                GetAssemblyData(
                    assembly,
                    out var assemblyAssemblySources,
                    out var assemblyReflectionBuilder,
                    out var assemblyServiceDescriptorBuilder,
                    assemblySymbols
                );

                internalAssemblyRequestsBuilder.AddRange(assemblyAssemblySources);
                internalReflectionRequestsBuilder.AddRange(assemblyReflectionBuilder);
                internalServiceDescriptorRequestsBuilder.AddRange(assemblyServiceDescriptorBuilder);

                // steps
                // cache requests resulting from an assembly.
                // cache results of assembly requests directly.
                //
            }
            catch (Exception e)
            {
                globalDiagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.UnhandledException,
                        assembly.Locations.FirstOrDefault(),
                        e.Message.Replace("\r", "").Replace("\n", ""),
                        e.StackTrace.Replace("\r", "").Replace("\n", ""),
                        e.GetType().Name,
                        e.ToString()
                    )
                );
            }
        }

        var internalReflectionRequests = internalReflectionRequestsBuilder.ToImmutable();
        var internalServiceDescriptorRequests = internalServiceDescriptorRequestsBuilder.ToImmutable();
        var diagnostics = new HashSet<Diagnostic>();
        foreach (var assembly in assemblySymbols.Values.Except([compilation.Assembly]).OrderBy(z => z.MetadataName))
        {
            foreach (var request in reflectionRequests.Concat(internalReflectionRequests))
            {
                var source = CacheSourceLocation(
                    request.Location,
                    assembly,
                    SourceLocationKind.Reflection,
                    () => ReflectionCollection.ResolveSource(
                        compilation,
                        diagnostics,
                        request,
                        assembly
                    )
                );
                if (source is { }) reflectionSources.Add(source);
            }

            foreach (var request in serviceDescriptorRequests.Concat(internalServiceDescriptorRequests))
            {
                var source = CacheSourceLocation(
                    request.Location,
                    assembly,
                    SourceLocationKind.ServiceDescriptor,
                    () => ServiceDescriptorCollection.ResolveSource(
                        compilation,
                        diagnostics,
                        request,
                        assembly
                    )
                );
                if (source is { }) serviceDescriptorSources.Add(source);
            }
        }

        globalDiagnostics.UnionWith(diagnostics);

        var result = (
            internalAssemblyRequestsBuilder.ToImmutable(),
            internalReflectionRequests,
            reflectionSources.ToImmutable(),
            internalServiceDescriptorRequests,
            serviceDescriptorSources.ToImmutable()
        );

        return result;
    }

#pragma warning disable RS1035
    private void GetAssemblyData(
        IAssemblySymbol assembly,
        out ImmutableList<AssemblyCollection.Item> assemblyItems,
        out ImmutableList<ReflectionCollection.Item> reflection,
        out ImmutableList<ServiceDescriptorCollection.Item> serviceDescriptor,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols
    )
    {
        var skipKey = (assembly.MetadataName + Constants.SkipExtension);
        if (skip.Contains(skipKey))
        {
            assemblyItems = [];
            reflection = [];
            serviceDescriptor = [];
            return;
        }

        var cacheKey = (assembly.MetadataName + Constants.AssemblyJsonExtension);
        if (generatedJson.TryGetValue(cacheKey, out var generatedData))
        {
            assemblyItems = generatedData.InternalAssemblyRequests.Select(z => GetAssembliesFromData(assemblySymbols, z)).ToImmutableList();
            reflection = generatedData.InternalReflectionRequests.Select(z => GetReflectionFromData(compilation, assemblySymbols, z)).ToImmutableList();
            serviceDescriptor =
                generatedData.InternalServiceDescriptorRequests.Select(z => GetServiceDescriptorFromData(compilation, assemblySymbols, z)).ToImmutableList();
            return;
        }

        var assemblyBuilder = ImmutableList.CreateBuilder<AssemblyCollection.Item>();
        var reflectionBuilder = ImmutableList.CreateBuilder<ReflectionCollection.Item>();
        var serviceDescriptorBuilder = ImmutableList.CreateBuilder<ServiceDescriptorCollection.Item>();
        var attributes = assembly.GetAttributes();
        foreach (var attribute in attributes)
        {
            if (attribute is not { AttributeClass.MetadataName: "AssemblyMetadataAttribute" }) continue;

            try
            {
                switch (attribute)
                {
                    case { ConstructorArguments: [{ Value: AssembliesKey }, { Value: string getAssembliesData }] }:
                        {
                            {
                                var data = GetAssembliesFromString(assemblySymbols, getAssembliesData);
                                assemblyBuilder.Add(data);
                            }
                            break;
                        }

                    case { ConstructorArguments: [{ Value: ReflectionTypesKey }, { Value: string reflectionData }] }:
                        {
                            {
                                var data = GetReflectionFromString(compilation, assemblySymbols, reflectionData);
                                reflectionBuilder.Add(data);
                            }
                            break;
                        }

                    case { ConstructorArguments: [{ Value: ServiceDescriptorTypesKey }, { Value: string serviceDescriptorData }] }:
                        {
                            {
                                var data = GetServiceDescriptorFromString(compilation, assemblySymbols, serviceDescriptorData);
                                serviceDescriptorBuilder.Add(data);
                            }
                            break;
                        }
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

        assemblyItems = assemblyBuilder.ToImmutable();
        reflection = reflectionBuilder.ToImmutable();
        serviceDescriptor = serviceDescriptorBuilder.ToImmutable();

        if (_cacheDirectory.Value is null || File.Exists(Path.Combine(_cacheDirectory.Value, cacheKey))) return;

        var result = new CompiledAssemblyProviderData(
            assemblyBuilder.Select(GetAssembyCollectionData).ToImmutableList(),
            reflectionBuilder.Select(GetReflectionCollectionData).ToImmutableList(),
            serviceDescriptorBuilder.Select(GetServiceDescriptorCollectionData).ToImmutableList()
        );

        if (result.IsEmpty)
            File.WriteAllText(Path.Combine(_cacheDirectory.Value, skipKey), string.Empty);
        else
            File.WriteAllText(
                Path.Combine(_cacheDirectory.Value, cacheKey),
                JsonSerializer.Serialize(result, JsonSourceGenerationContext.Default.CompiledAssemblyProviderData)
            );
    }
#pragma warning restore RS1035

    public static IEnumerable<AttributeListSyntax> ToAssemblyAttributes(
        ImmutableList<AssemblyCollection.Item> assemblyRequests,
        ImmutableList<ReflectionCollection.Item> reflectionTypes,
        ImmutableList<ServiceDescriptorCollection.Item> serviceDescriptorTypes
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
        var config = JsonSerializer.Deserialize(result, JsonSourceGenerationContext.Default.GetAssemblyConfiguration)!;
        Debug.Assert(config.Type == nameof(JsonSourceGenerationContext.Default.GetAssemblyConfiguration));
        return GetAssembliesFromData(assemblySymbols, config);
    }

    private static AssemblyCollection.Item GetAssembliesFromData(ImmutableDictionary<string, IAssemblySymbol> assemblySymbols, GetAssemblyConfiguration config)
    {
        var assemblyFilter = LoadAssemblyFilter(config.Assembly.Assembly, config.Assembly.Location, assemblySymbols);
        return new(config.Assembly.Location, assemblyFilter);
    }

    private static string GetAssembliesToString(AssemblyCollection.Item item)
    {
        var data = GetAssembyCollectionData(item);
        var result = JsonSerializer.SerializeToUtf8Bytes(data, JsonSourceGenerationContext.Default.GetAssemblyConfiguration);
        return CompressString(result);
    }

    private static GetAssemblyConfiguration GetAssembyCollectionData(AssemblyCollection.Item item)
    {
        var data = new GetAssemblyConfiguration(new(item.Location, LoadAssemblyFilterData(item.AssemblyFilter)));
        return data;
    }

    private static ReflectionCollection.Item GetReflectionFromString(
        Compilation compilation,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols,
        string value
    )
    {
        var result = DecompressString(value);
        // ReSharper disable once NullableWarningSuppressionIsUsed
        var config = JsonSerializer.Deserialize(result, JsonSourceGenerationContext.Default.GetReflectionCollectionData)!;
        Debug.Assert(config.Type == nameof(JsonSourceGenerationContext.Default.GetReflectionCollectionData));
        return GetReflectionFromData(compilation, assemblySymbols, config);
    }

    private static ReflectionCollection.Item GetReflectionFromData(Compilation compilation, ImmutableDictionary<string, IAssemblySymbol> assemblySymbols, GetReflectionCollectionData config)
    {
        var data = config.Assembly;
        var assemblyFilter = LoadAssemblyFilter(data.Assembly, data.Location, assemblySymbols);
        var typeFilter = LoadTypeFilter(compilation, config.Reflection.Type, data.Location, assemblySymbols);
        return new(data.Location, assemblyFilter, typeFilter);
    }

    private static string GetReflectionToString(ReflectionCollection.Item item)
    {
        var data = GetReflectionCollectionData(item);
        var result = JsonSerializer.SerializeToUtf8Bytes(data, JsonSourceGenerationContext.Default.GetReflectionCollectionData);
        return CompressString(result);
    }

    private static GetReflectionCollectionData GetReflectionCollectionData(ReflectionCollection.Item item)
    {
        var data = new GetReflectionCollectionData(
            new(item.Location, LoadAssemblyFilterData(item.AssemblyFilter)),
            new(LoadTypeFilterData(item.TypeFilter))
        );
        return data;
    }

    private static ServiceDescriptorCollection.Item GetServiceDescriptorFromString(
        Compilation compilation,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols,
        string value
    )
    {
        var result = DecompressString(value);
        // ReSharper disable once NullableWarningSuppressionIsUsed
        var config = JsonSerializer.Deserialize(result, JsonSourceGenerationContext.Default.GetServiceDescriptorCollectionData)!;
        Debug.Assert(config.Type == nameof(JsonSourceGenerationContext.Default.GetServiceDescriptorCollectionData));
        return GetServiceDescriptorFromData(compilation, assemblySymbols, config);
    }

    private static ServiceDescriptorCollection.Item GetServiceDescriptorFromData(Compilation compilation, ImmutableDictionary<string, IAssemblySymbol> assemblySymbols, GetServiceDescriptorCollectionData config)
    {
        var assemblyData = config.Assembly;
        var reflectionData = config.Reflection;
        var serviceDescriptorData = config.ServiceDescriptor;
        var assemblyFilter = LoadAssemblyFilter(assemblyData.Assembly, assemblyData.Location, assemblySymbols);
        var typeFilter = LoadTypeFilter(compilation, reflectionData.Type, assemblyData.Location, assemblySymbols);
        // next up is services, and lifetime.
        var servicesFilter = LoadServiceDescriptorFilter(compilation, serviceDescriptorData.ServiceDescriptor, assemblyData.Location, assemblySymbols);
        return new(assemblyData.Location, assemblyFilter, typeFilter, servicesFilter, serviceDescriptorData.Lifetime);
    }

    private static string GetServiceDescriptorToString(ServiceDescriptorCollection.Item item)
    {
        var data = GetServiceDescriptorCollectionData(item);
        var result = JsonSerializer.SerializeToUtf8Bytes(data, JsonSourceGenerationContext.Default.GetServiceDescriptorCollectionData);
        return CompressString(result);
    }

    private static GetServiceDescriptorCollectionData GetServiceDescriptorCollectionData(ServiceDescriptorCollection.Item item)
    {
        var data = new GetServiceDescriptorCollectionData(
            new(item.Location, LoadAssemblyFilterData(item.AssemblyFilter)),
            new(LoadTypeFilterData(item.TypeFilter)),
            new(LoadServiceDescriptorsData(item.ServicesTypeFilter), item.Lifetime)
        );
        return data;
    }

    private static byte[] DecompressString(string base64String) => Convert.FromBase64String(base64String);

    private static string CompressString(byte[] bytes) => Convert.ToBase64String(bytes);

    private static AssemblyFilterData LoadAssemblyFilterData(CompiledAssemblyFilter filter) => new(
        filter.AssemblyDescriptors.OfType<AllAssemblyDescriptor>().Any(),
        filter.AssemblyDescriptors.OfType<IncludeSystemAssembliesDescriptor>().Any(),
        [.. filter.AssemblyDescriptors.OfType<AssemblyDescriptor>().Select(z => z.Assembly.MetadataName).OrderBy(z => z)],
        [.. filter.AssemblyDescriptors.OfType<NotAssemblyDescriptor>().Select(z => z.Assembly.MetadataName).OrderBy(z => z)],
        [.. filter.AssemblyDescriptors.OfType<AssemblyDependenciesDescriptor>().Select(z => z.Assembly.MetadataName).OrderBy(z => z)]
    );

    private static TypeFilterData LoadTypeFilterData(CompiledTypeFilter typeFilter) => new(
        typeFilter.ClassFilter,
        typeFilter
           .TypeFilterDescriptors
           .OfType<NamespaceFilterDescriptor>()
           .Select(static z => new NamespaceFilterData(z.Filter, [.. z.Namespaces.OrderBy(z => z)]))
           .OrderBy(static z => string.Join(",", z.Namespaces.OrderBy(static z => z)))
           .ThenBy(static z => z.Filter)
           .Select(static z => z with { Namespaces = [.. z.Namespaces.OrderBy(z => z)] })
           .ToImmutableArray(),
        [
            .. typeFilter
              .TypeFilterDescriptors.OfType<NameFilterDescriptor>()
              .Select(static z => new NameFilterData(z.Include, z.Filter, [.. z.Names.OrderBy(z => z)]))
              .OrderBy(static z => string.Join(",", z.Names.OrderBy(static z => z)))
              .ThenBy(static z => z.Filter)
        ],
        [
            .. typeFilter
              .TypeFilterDescriptors.OfType<TypeKindFilterDescriptor>()
              .Select(static z => new TypeKindFilterData(z.Include, [.. z.TypeKinds.OrderBy(z => z)]))
              .OrderBy(static z => string.Join(",", z.TypeKinds.OrderBy(static z => z)))
              .ThenBy(static z => z.Include)
        ],
        [
            .. typeFilter
              .TypeFilterDescriptors.OfType<TypeInfoFilterDescriptor>()
              .Select(static z => new TypeInfoFilterData(z.Include, [.. z.TypeInfos.OrderBy(z => z)]))
              .OrderBy(static z => string.Join(",", z.TypeInfos.OrderBy(static z => z)))
              .ThenBy(static z => z.Include)
        ],
        [
            .. typeFilter
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
        ],
        [
            .. typeFilter
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
        ],
        [
            .. typeFilter
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
        ],
        [
            .. typeFilter
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
        ],
        [
            .. typeFilter
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
        ],
        [
            .. typeFilter
              .TypeFilterDescriptors
              .Select(
                   f => f switch
                        {
                            AssignableToAnyTypeFilterDescriptor descriptor => new(
                                true,
                                [
                                    .. descriptor
                                      .Types.Select(
                                           z => new AnyTypeData(z.ContainingAssembly.MetadataName, Helpers.GetFullMetadataName(z), z.IsUnboundGenericType)
                                       )
                                      .OrderBy(z => z.Assembly)
                                      .ThenBy(z => z.Type)
                                ]
                            ),
                            NotAssignableToAnyTypeFilterDescriptor descriptor => new AssignableToAnyTypeData(
                                false,
                                [
                                    .. descriptor
                                      .Types
                                      .Select(z => new AnyTypeData(z.ContainingAssembly.MetadataName, Helpers.GetFullMetadataName(z), z.IsUnboundGenericType))
                                      .OrderBy(z => z.Assembly)
                                      .ThenBy(z => z.Type)
                                ]
                            ),
                            _ => null!,
                        }
               )
              .Where(z => z is { })
              .OrderBy(z => string.Join(",", z.Types))
              .ThenBy(z => z.Include)
        ]
    );

    private static CompiledAssemblyFilter LoadAssemblyFilter(
        AssemblyFilterData data,
        SourceLocation source,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols
    )
    {
        var descriptors = ImmutableList.CreateBuilder<IAssemblyDescriptor>();
        if (data.AllAssembly) descriptors.Add(new AllAssemblyDescriptor());

        if (data.IncludeSystem) descriptors.Add(new IncludeSystemAssembliesDescriptor());

        foreach (var item in data.Assembly)
        {
            if (!assemblySymbols.TryGetValue(item, out var assembly)) continue;
            descriptors.Add(new AssemblyDescriptor(assembly));
        }

        foreach (var item in data.NotAssembly)
        {
            if (!assemblySymbols.TryGetValue(item, out var assembly)) continue;
            descriptors.Add(new NotAssemblyDescriptor(assembly));
        }

        foreach (var item in data.AssemblyDependencies)
        {
            if (!assemblySymbols.TryGetValue(item, out var assembly)) continue;
            descriptors.Add(new AssemblyDependenciesDescriptor(assembly));
        }

        return new(descriptors.ToImmutable(), source);
    }

    private static CompiledTypeFilter LoadTypeFilter(
        Compilation compilation,
        TypeFilterData data,
        SourceLocation source,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols
    )
    {
        var descriptors = ImmutableList.CreateBuilder<ITypeFilterDescriptor>();
        foreach (var item in data.NamespaceFilters)
        {
            descriptors.Add(new NamespaceFilterDescriptor(item.Filter, [.. item.Namespaces]));
        }

        foreach (var item in data.NameFilters)
        {
            descriptors.Add(new NameFilterDescriptor(item.Include, item.Filter, [.. item.Names]));
        }

        foreach (var item in data.TypeKindFilters)
        {
            descriptors.Add(new TypeKindFilterDescriptor(item.Include, [.. item.TypeKinds]));
        }

        foreach (var item in data.TypeInfoFilters)
        {
            descriptors.Add(new TypeInfoFilterDescriptor(item.Include, [.. item.TypeInfos]));
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
                item.Include
                    ? new WithAttributeStringFilterDescriptor(item.Attribute)
                    : new WithoutAttributeStringFilterDescriptor(item.Attribute)
            );
        }

        var withAnyAttributeFilter = new List<INamedTypeSymbol>();
        foreach (var item in data.WithAnyAttributeFilters)
        {
            if (findType(assemblySymbols, compilation, item.Assembly, item.Attribute) is not { } type) continue;
            if (item.UnboundGenericType) type = type.ConstructUnboundGenericType();
            withAnyAttributeFilter.Add(type);
        }

        if (withAnyAttributeFilter.Any()) descriptors.Add(new WithAnyAttributeFilterDescriptor(withAnyAttributeFilter.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)));

        var withAnyAttributeStringFilter = new List<string>();
        foreach (var item in data.WithAnyAttributeStringFilters)
        {
            withAnyAttributeStringFilter.Add(item.Attribute);
        }

        if (withAnyAttributeStringFilter.Any()) descriptors.Add(new WithAnyAttributeStringFilterDescriptor([.. withAnyAttributeStringFilter]));

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

        return new(data.Filter, descriptors.ToImmutable(), source);
    }

    private static INamedTypeSymbol? findType(
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols,
        Compilation compilation,
        string assemblyName,
        string typeName
    )
    {
        return CompiledAssemblyFilter.coreAssemblies.Contains(assemblyName)
            ? compilation.GetTypeByMetadataName(typeName)
            : !assemblySymbols.TryGetValue(assemblyName, out var assembly)
         || FindTypeVisitor.FindType(compilation, assembly, typeName) is not { } type
                ? compilation.GetTypeByMetadataName(typeName)
                : type;
    }

    private static ServiceDescriptorFilterData LoadServiceDescriptorsData(
        CompiledServiceTypeDescriptors serviceTypeDescriptors
    )
    {
        var serviceDescriptors = serviceTypeDescriptors.ServiceTypeDescriptors.Select(
            z => z switch
                 {
                     ImplementedInterfacesServiceTypeDescriptor i => new(
                         'i',
                         TypeFilter: i.InterfaceFilter is { } ? LoadTypeFilterData(i.InterfaceFilter) : null
                     ),
                     MatchingInterfaceServiceTypeDescriptor => new('m'),
                     SelfServiceTypeDescriptor => new('s'),
                     AsTypeFilterServiceTypeDescriptor => new('a'),
                     CompiledServiceTypeDescriptor c => new ServiceTypeData(
                         'c',
                         new(c.Type.ContainingAssembly.MetadataName, c.Type.MetadataName, c.Type.IsUnboundGenericType)
                     ),
                     _ => throw new ArgumentOutOfRangeException(nameof(z)),
                 }
        );
        return new(serviceDescriptors.ToImmutableArray(), serviceTypeDescriptors.Lifetime);
    }

    private static CompiledServiceTypeDescriptors LoadServiceDescriptorFilter(
        Compilation compilation,
        ServiceDescriptorFilterData data,
        SourceLocation source,
        ImmutableDictionary<string, IAssemblySymbol> assemblySymbols
    )
    {
        var descriptors = ImmutableArray.CreateBuilder<IServiceTypeDescriptor>();
        foreach (var item in data.ServiceTypeDescriptors)
        {
            descriptors.Add(
                item switch
                {
                    { Identifier: 'c', TypeData: { } typeData } =>
                        new CompiledServiceTypeDescriptor(findType(assemblySymbols, compilation, typeData.Assembly, typeData.Type)!),
                    { Identifier: 'i', TypeFilter: { } typeFilter } =>
                        new ImplementedInterfacesServiceTypeDescriptor(LoadTypeFilter(compilation, typeFilter, source, assemblySymbols)),
                    { Identifier: 'i' } => new ImplementedInterfacesServiceTypeDescriptor(null),
                    { Identifier: 'm' } => new MatchingInterfaceServiceTypeDescriptor(),
                    { Identifier: 's' } => new SelfServiceTypeDescriptor(),
                    { Identifier: 'a' } => new AsTypeFilterServiceTypeDescriptor(),
                    _ => throw new ArgumentOutOfRangeException(nameof(data)),
                }
            );
        }

        return new(descriptors.ToImmutable(), data.Lifetime);
    }
}
