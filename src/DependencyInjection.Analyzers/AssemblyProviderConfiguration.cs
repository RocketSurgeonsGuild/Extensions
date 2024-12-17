using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

// ReSharper disable UseCollectionExpression

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal partial class AssemblyProviderConfiguration
(
    SourceProductionContext context,
    Compilation compilation,
    AnalyzerConfigOptionsProvider options)
{
    #pragma warning disable RS1035
    private readonly Lazy<string?> _cacheDirectory = new(
        () =>
        {
            var directory = ( options.GlobalOptions.TryGetValue("build_property.BaseIntermediateOutputPath", out var intermediateOutputPath) )
                ? intermediateOutputPath
                : null;
            if (!Path.IsPathRooted(directory) && options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDirectory)) directory = Path.Combine(projectDirectory, directory);

            if (directory is null) return null;

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var cacheDirectory = Path.Combine(directory, "GeneratedAssemblyProvider");
            if (!Directory.Exists(cacheDirectory)) Directory.CreateDirectory(cacheDirectory);

            return cacheDirectory;
        }
    );
    #pragma warning restore RS1035

    private static string GetCacheFileHash(SourceLocation location)
    {
        using var hasher = MD5.Create();
        hasher.ComputeHash(Encoding.UTF8.GetBytes(location.FileName));
        hasher.ComputeHash(Encoding.UTF8.GetBytes(location.ExpressionHash));
        hasher.ComputeHash(Encoding.UTF8.GetBytes(location.LineNumber.ToString()));
        return hasher.Hash.Aggregate("", (s, b) => s + b.ToString("x2"));
    }

    #pragma warning disable RS1035
    private ResolvedSourceLocation? CacheSourceLocation(SourceLocation location, Func<ResolvedSourceLocation?> factory)
    {
        var cacheKey = $"compilation-{GetCacheFileHash(location)}.partial";
        if (_cacheDirectory.Value is { } && File.Exists(Path.Combine(_cacheDirectory.Value, cacheKey))) return new(location, File.ReadAllText(Path.Combine(_cacheDirectory.Value, cacheKey)), []);

        var source = factory();
        if (_cacheDirectory.Value is { } && !File.Exists(Path.Combine(_cacheDirectory.Value, cacheKey))) File.WriteAllText(Path.Combine(_cacheDirectory.Value, cacheKey), source?.Expression ?? "");

        return source;
    }
    #pragma warning restore RS1035

    public (
        ImmutableList<ResolvedSourceLocation> AssemblySources,
        ImmutableList<ReflectionCollection.Item> InternalReflectionRequests,
        ImmutableList<ResolvedSourceLocation> ReflectionSources,
        ImmutableList<ServiceDescriptorCollection.Item> InternalServiceDescriptorRequests,
        ImmutableList<ResolvedSourceLocation> ServiceDescriptorSources
        ) FromAssemblyAttributes(
            ImmutableList<ReflectionCollection.Item> reflectionRequests,
            ImmutableList<ServiceDescriptorCollection.Item> serviceDescriptorRequests,
            HashSet<IAssemblySymbol> globalPrivateAssemblies,
            HashSet<Diagnostic> globalDiagnostics
        )
    {
        var assemblySources = ImmutableList.CreateBuilder<ResolvedSourceLocation>();
        var reflectionSources = ImmutableList.CreateBuilder<ResolvedSourceLocation>();
        var serviceDescriptorSources = ImmutableList.CreateBuilder<ResolvedSourceLocation>();
        {
            var diagnostics = new HashSet<Diagnostic>();
            var privateAssemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
            foreach (var request in reflectionRequests)
            {
                var source = CacheSourceLocation(
                    request.Location,
                    () => ReflectionCollection.ResolveSource(
                        compilation,
                        diagnostics,
                        request,
                        privateAssemblies,
                        (c, visitor) => visitor.GetReferencedTypes(c)
                    )
                );
                if (source is { }) reflectionSources.Add(source);
            }

            foreach (var request in serviceDescriptorRequests)
            {
                var source = CacheSourceLocation(
                    request.Location,
                    () => ServiceDescriptorCollection.ResolveSource(
                        compilation,
                        diagnostics,
                        request,
                        privateAssemblies,
                        (c, visitor) => visitor.GetReferencedTypes(c)
                    )
                );
                if (source is { }) serviceDescriptorSources.Add(source);
            }

            globalPrivateAssemblies.UnionWith(privateAssemblies);
            globalDiagnostics.UnionWith(diagnostics);
        }

        var assemblySymbols = compilation
                             .References.Select(compilation.GetAssemblyOrModuleSymbol)
                              //.Concat([compilation.Assembly])
                             .Select(
                                  symbol =>
                                  {
                                      if (symbol is IAssemblySymbol assemblySymbol) return assemblySymbol;

                                      if (symbol is IModuleSymbol moduleSymbol) return moduleSymbol.ContainingAssembly;

                                      // ReSharper disable once NullableWarningSuppressionIsUsed
                                      return null!;
                                  }
                              )
                             .Where(z => z is { })
                             .GroupBy(z => z.MetadataName, z => z, (s, symbols) => ( Key: s, Symbol: symbols.First() ))
                             .ToImmutableDictionary(z => z.Key, z => z.Symbol);


        var internalReflectionRequestsBuilder = ImmutableList.CreateBuilder<ReflectionCollection.Item>();
        var internalServiceDescriptorRequestsBuilder = ImmutableList.CreateBuilder<ServiceDescriptorCollection.Item>();
        foreach (var assembly in assemblySymbols.Values)
        {
            GetAssemblyData(
                assembly,
                out var assemblyAssemblySources,
                out var assemblyReflectionSources,
                out var assemblyServiceDescriptorSources,
                out var assemblyReflectionBuilder,
                out var assemblyServiceDescriptorBuilder,
                out var privateAssemblies,
                out var assemblyDiagnostics
            );

            internalReflectionRequestsBuilder.AddRange(assemblyReflectionBuilder);
            internalServiceDescriptorRequestsBuilder.AddRange(assemblyServiceDescriptorBuilder);

            assemblySources.AddRange(assemblyAssemblySources);
            reflectionSources.AddRange(assemblyReflectionSources);
            serviceDescriptorSources.AddRange(assemblyServiceDescriptorSources);
            globalPrivateAssemblies.UnionWith(privateAssemblies);
            globalDiagnostics.UnionWith(assemblyDiagnostics);

            // steps
            // cache requests resulting from an assembly.
            // cache results of assembly requests directly.
            //
        }

        var internalReflectionRequests = internalReflectionRequestsBuilder.ToImmutable();
        var internalServiceDescriptorRequests = internalServiceDescriptorRequestsBuilder.ToImmutable();

        var result = (
            assemblySources.ToImmutable(),
            internalReflectionRequests,
            reflectionSources.ToImmutable(),
            internalServiceDescriptorRequests,
            serviceDescriptorSources.ToImmutable()
        );

        return result;

        #pragma warning disable RS1035
        void GetAssemblyData(
            IAssemblySymbol assembly,
            out ImmutableList<ResolvedSourceLocation> assemblyAssemblySources,
            out ImmutableList<ResolvedSourceLocation> assemblyReflectionSources,
            out ImmutableList<ResolvedSourceLocation> assemblyServiceDescriptorSources,
            out ImmutableList<ReflectionCollection.Item> reflection,
            out ImmutableList<ServiceDescriptorCollection.Item> serviceDescriptor,
            out ImmutableHashSet<IAssemblySymbol> privateAssemblies,
            out ImmutableHashSet<Diagnostic> assemblyDiagnostics
        )
        {
            var diagnostics = new HashSet<Diagnostic>();
            var cacheKey = assembly.MetadataName + ".json";
            if (_cacheDirectory.Value is { } && File.Exists(Path.Combine(_cacheDirectory.Value, cacheKey)))
            {
                var data = JsonSerializer.Deserialize(
                    File.ReadAllText(Path.Combine(_cacheDirectory.Value, cacheKey)),
                    SourceGenerationContext.Default.CompiledAssemblyProviderData
                );
                assemblyAssemblySources = data.AssemblySources;
                assemblyReflectionSources = data.ReflectionSources;
                assemblyServiceDescriptorSources = data.ServiceDescriptorSources;
                reflection = data.InternalReflectionRequests.Select(z => GetReflectionFromString(compilation, assemblySymbols, z)).ToImmutableList();
                serviceDescriptor =
                    data.InternalServiceDescriptorRequests.Select(z => GetServiceDescriptorFromString(compilation, assemblySymbols, z)).ToImmutableList();
                privateAssemblies = data
                                   .PrivateAssemblyNames
                                   .Select(z => assemblySymbols[z])
                                   .ToImmutableHashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
                assemblyDiagnostics = [.. diagnostics];
                return;
                //                return new(location, File.ReadAllText(Path.Combine(_cacheDirectory, cacheKey)));
            }

            var assemblyAssemblySourcesBuilder = ImmutableList.CreateBuilder<ResolvedSourceLocation>();
            var assemblyReflectionSourcesBuilder = ImmutableList.CreateBuilder<ResolvedSourceLocation>();
            var assemblyServiceDescriptorSourcesBuilder = ImmutableList.CreateBuilder<ResolvedSourceLocation>();
            var reflectionBuilder = ImmutableList.CreateBuilder<ReflectionCollection.Item>();
            var serviceDescriptorBuilder = ImmutableList.CreateBuilder<ServiceDescriptorCollection.Item>();
            var attributes = assembly.GetAttributes();
            HashSet<IAssemblySymbol> assemblies = new(SymbolEqualityComparer.Default);
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
                                    var source = CacheSourceLocation(
                                        data.Location,
                                        () => AssemblyCollection.ResolveSources(compilation, diagnostics, [data], assemblies).SingleOrDefault()
                                    );
                                    if (source is { }) assemblyAssemblySourcesBuilder.Add(source);
                                }
                                break;
                            }

                        case { ConstructorArguments: [{ Value: ReflectionTypesKey }, { Value: string reflectionData }] }:
                            {
                                {
                                    var data = GetReflectionFromString(compilation, assemblySymbols, reflectionData);
                                    var source = CacheSourceLocation(
                                        data.Location,
                                        () => ReflectionCollection.ResolveSource(
                                            compilation,
                                            diagnostics,
                                            data,
                                            assemblies,
                                            (c, visitor) => visitor.GetReferencedTypes(c)
                                        )
                                    );
                                    if (source is { }) assemblyReflectionSourcesBuilder.Add(source);

                                    reflectionBuilder.Add(data);
                                }
                                break;
                            }

                        case { ConstructorArguments: [{ Value: ServiceDescriptorTypesKey }, { Value: string serviceDescriptorData }] }:
                            {
                                {
                                    var data = GetServiceDescriptorFromString(compilation, assemblySymbols, serviceDescriptorData);
                                    var source = CacheSourceLocation(
                                        data.Location,
                                        () => ServiceDescriptorCollection.ResolveSource(
                                            compilation,
                                            diagnostics,
                                            data,
                                            assemblies,
                                            (c, visitor) => visitor.GetReferencedTypes(c)
                                        )
                                    );
                                    if (source is { }) assemblyServiceDescriptorSourcesBuilder.Add(source);

                                    serviceDescriptorBuilder.Add(data);
                                }
                                break;
                            }

                        default:
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

            assemblyAssemblySources = assemblyAssemblySourcesBuilder.ToImmutable();
            assemblyReflectionSources = assemblyReflectionSourcesBuilder.ToImmutable();
            assemblyServiceDescriptorSources = assemblyServiceDescriptorSourcesBuilder.ToImmutable();
            reflection = reflectionBuilder.ToImmutable();
            serviceDescriptor = serviceDescriptorBuilder.ToImmutable();
            privateAssemblies = assemblies.ToImmutableHashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
            assemblyDiagnostics = [.. diagnostics];

            if (_cacheDirectory.Value is { } && !File.Exists(Path.Combine(_cacheDirectory.Value, cacheKey)))
            {
                var data = new CompiledAssemblyProviderData(
                    assemblyAssemblySources,
                    assemblyReflectionSources,
                    assemblyServiceDescriptorSources,
                    reflection.Select(GetReflectionToString).ToImmutableList(),
                    serviceDescriptor.Select(GetServiceDescriptorToString).ToImmutableList(),
                    assemblies
                       .Select(z => z.MetadataName)
                       .Concat(assemblyAssemblySources.SelectMany(z => z.PrivateAssemblies))
                       .Concat(assemblyReflectionSources.SelectMany(z => z.PrivateAssemblies))
                       .Concat(assemblyServiceDescriptorSources.SelectMany(z => z.PrivateAssemblies))
                       .ToImmutableHashSet(),
                    assemblyDiagnostics
                );

                File.WriteAllText(
                    Path.Combine(_cacheDirectory.Value, cacheKey),
                    JsonSerializer.Serialize(data, SourceGenerationContext.Default.CompiledAssemblyProviderData)
                );
            }
        }
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
        var config = JsonSerializer.Deserialize(result, SourceGenerationContext.Default.GetAssemblyConfiguration)!;
        Debug.Assert(config.Type == nameof(SourceGenerationContext.Default.GetAssemblyConfiguration));
        var data = config.Assembly;
        var assemblyFilter = LoadAssemblyFilter(data.Assembly, data.Location, assemblySymbols);
        return new(data.Location, assemblyFilter);
    }

    private static string GetAssembliesToString(AssemblyCollection.Item item)
    {
        var data = new AssemblyCollectionData(item.Location, LoadAssemblyFilterData(item.AssemblyFilter));
        var result = JsonSerializer.SerializeToUtf8Bytes(data, SourceGenerationContext.Default.GetAssemblyConfiguration);
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
        var config = JsonSerializer.Deserialize(result, SourceGenerationContext.Default.GetReflectionCollectionData)!;
        Debug.Assert(config.Type == nameof(SourceGenerationContext.Default.GetReflectionCollectionData));
        var data = config.Assembly;
        var assemblyFilter = LoadAssemblyFilter(data.Assembly, data.Location, assemblySymbols);
        var typeFilter = LoadTypeFilter(compilation, config.Reflection.Type, data.Location, assemblySymbols);
        return new(data.Location, assemblyFilter, typeFilter);
    }

    private static string GetReflectionToString(ReflectionCollection.Item item)
    {
        var data = new GetReflectionCollectionData(
            new(item.Location, LoadAssemblyFilterData(item.AssemblyFilter)),
            new(LoadTypeFilterData(item.TypeFilter))
        );
        var result = JsonSerializer.SerializeToUtf8Bytes(data, SourceGenerationContext.Default.GetReflectionCollectionData);
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
        var config = JsonSerializer.Deserialize(result, SourceGenerationContext.Default.GetServiceDescriptorCollectionData)!;
        Debug.Assert(config.Type == nameof(SourceGenerationContext.Default.GetServiceDescriptorCollectionData));
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
        var data = new GetServiceDescriptorCollectionData(
            new(item.Location, LoadAssemblyFilterData(item.AssemblyFilter)),
            new(LoadTypeFilterData(item.TypeFilter)),
            new(LoadServiceDescriptorsData(item.ServicesTypeFilter), item.Lifetime)
        );
        var result = JsonSerializer.SerializeToUtf8Bytes(data, SourceGenerationContext.Default.GetServiceDescriptorCollectionData);
        return CompressString(result);
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
           .Select(z => new NamespaceFilterData(z.Filter, [.. z.Namespaces.OrderBy(z => z)]))
           .OrderBy(z => string.Join(",", z.Namespaces.OrderBy(static z => z)))
           .ThenBy(z => z.Filter)
           .Select(z => z with { Namespaces = [.. z.Namespaces.OrderBy(z => z)] })
           .ToImmutableArray(),
        [
            .. typeFilter
              .TypeFilterDescriptors.OfType<NameFilterDescriptor>()
              .Select(z => new NameFilterData(z.Include, z.Filter, [.. z.Names.OrderBy(z => z)]))
              .OrderBy(z => string.Join(",", z.Names.OrderBy(static z => z)))
              .ThenBy(z => z.Filter)
        ],
        [
            .. typeFilter
              .TypeFilterDescriptors.OfType<TypeKindFilterDescriptor>()
              .Select(z => new TypeKindFilterData(z.Include, [.. z.TypeKinds.OrderBy(z => z)]))
              .OrderBy(z => string.Join(",", z.TypeKinds.OrderBy(static z => z)))
              .ThenBy(z => z.Include)
        ],
        [
            .. typeFilter
              .TypeFilterDescriptors.OfType<TypeInfoFilterDescriptor>()
              .Select(z => new TypeInfoFilterData(z.Include, [.. z.TypeInfos.OrderBy(z => z)]))
              .OrderBy(z => string.Join(",", z.TypeInfos.OrderBy(static z => z)))
              .ThenBy(z => z.Include)
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
            descriptors.Add(( item.Include ) ? new WithAttributeFilterDescriptor(type) : new WithoutAttributeFilterDescriptor(type));
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
            descriptors.Add(new WithAnyAttributeFilterDescriptor(withAnyAttributeFilter.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)));

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

            descriptors.Add(( item.Include ) ? new AssignableToTypeFilterDescriptor(type) : new NotAssignableToTypeFilterDescriptor(type));
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
                ( item.Include )
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
        return ( CompiledAssemblyFilter.coreAssemblies.Contains(assemblyName) )
                ? compilation.GetTypeByMetadataName(typeName)
                : ( !assemblySymbols.TryGetValue(assemblyName, out var assembly)
                 || FindTypeVisitor.FindType(compilation, assembly, typeName) is not { } type )
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
                         TypeFilter: ( i.InterfaceFilter is { } ) ? LoadTypeFilterData(i.InterfaceFilter) : null
                     ),
                     MatchingInterfaceServiceTypeDescriptor => new('m'),
                     SelfServiceTypeDescriptor              => new('s'),
                     AsTypeFilterServiceTypeDescriptor      => new('a'),
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
                    _                   => throw new ArgumentOutOfRangeException(nameof(data)),
                }
            );
        }

        return new(descriptors.ToImmutable(), data.Lifetime);
    }

    private record ServiceTypeData(char Identifier, AnyTypeData? TypeData = null, TypeFilterData? TypeFilter = null);

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
    [JsonSerializable(typeof(CompiledAssemblyProviderData))]
    [JsonSerializable(typeof(GetAssemblyConfiguration))]
    [JsonSerializable(typeof(GetReflectionCollectionData))]
    [JsonSerializable(typeof(GetServiceDescriptorCollectionData))]
    private partial class SourceGenerationContext : JsonSerializerContext;

    private record GetAssemblyConfiguration
    (
        [property: JsonPropertyName("a")]
        AssemblyCollectionData Assembly
    )
    {
        public string Type => nameof(GetAssemblyConfiguration);
    };

    private record GetReflectionCollectionData
    (
        [property: JsonPropertyName("a")]
        AssemblyCollectionData Assembly,
        [property: JsonPropertyName("r")]
        ReflectionCollectionData Reflection
    )
    {
        public string Type => nameof(GetReflectionCollectionData);
    }

    private record GetServiceDescriptorCollectionData
    (
        [property: JsonPropertyName("a")]
        AssemblyCollectionData Assembly,
        [property: JsonPropertyName("r")]
        ReflectionCollectionData Reflection,
        [property: JsonPropertyName("s")]
        ServiceDescriptorCollectionData ServiceDescriptor)
    {
        public string Type => nameof(GetServiceDescriptorCollectionData);
    }

    private record AssemblyCollectionData
    (
        [property: JsonPropertyName("l")]
        SourceLocation Location,
        [property: JsonPropertyName("a")]
        AssemblyFilterData Assembly
    );

    private record ReflectionCollectionData
    (
        [property: JsonPropertyName("t")]
        TypeFilterData Type
    );

    private record ServiceDescriptorCollectionData
    (
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
        [property: JsonPropertyName("b")]
        ClassFilter Filter,
        [property: JsonPropertyName("c")]
        ImmutableArray<NamespaceFilterData> NamespaceFilters,
        [property: JsonPropertyName("d")]
        ImmutableArray<NameFilterData> NameFilters,
        [property: JsonPropertyName("e")]
        ImmutableArray<TypeKindFilterData> TypeKindFilters,
        [property: JsonPropertyName("f")]
        ImmutableArray<TypeInfoFilterData> TypeInfoFilters,
        [property: JsonPropertyName("g")]
        ImmutableArray<WithAttributeData> WithAttributeFilters,
        [property: JsonPropertyName("h")]
        ImmutableArray<WithAttributeStringData> WithAttributeStringFilters,
        [property: JsonPropertyName("i")]
        ImmutableArray<WithAttributeData> WithAnyAttributeFilters,
        [property: JsonPropertyName("j")]
        ImmutableArray<WithAttributeStringData> WithAnyAttributeStringFilters,
        [property: JsonPropertyName("k")]
        ImmutableArray<AssignableToTypeData> AssignableToTypeFilters,
        [property: JsonPropertyName("l")]
        ImmutableArray<AssignableToAnyTypeData> AssignableToAnyTypeFilters
    );

    private record ServiceDescriptorFilterData
    (
        ImmutableArray<ServiceTypeData> ServiceTypeDescriptors,
        int Lifetime
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
        [property: JsonPropertyName("i")]
        bool Include,
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

internal record CompiledAssemblyProviderData
(
    ImmutableList<ResolvedSourceLocation> AssemblySources,
    ImmutableList<ResolvedSourceLocation> ReflectionSources,
    ImmutableList<ResolvedSourceLocation> ServiceDescriptorSources,
    ImmutableList<string> InternalReflectionRequests,
    ImmutableList<string> InternalServiceDescriptorRequests,
    ImmutableHashSet<string> PrivateAssemblyNames,
    ImmutableHashSet<Diagnostic> Diagnostics
);
