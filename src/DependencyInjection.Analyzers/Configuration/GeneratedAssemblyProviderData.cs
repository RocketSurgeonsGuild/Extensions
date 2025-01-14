using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record GeneratedAssemblyProviderData
(
    ImmutableDictionary<string, CompiledAssemblyProviderData> AssemblyData,
    ImmutableHashSet<string> SkipAssemblies,
    ImmutableDictionary<string, GeneratedLocationAssemblyResolvedSourceCollection> Partials
)
{
    public CompiledAssemblyProviderData? GetAssemblyData(IAssemblySymbol assembly) =>
        AssemblyData.TryGetValue(assembly.MetadataName, out var data) && assembly.MatchesCachedVersion(data.CacheVersion)
            ? data
            : null;

    public ResolvedSourceLocation? GetSourceLocation(IAssemblySymbol assembly, SourceLocation sourceLocation, Func<ResolvedSourceLocation?> factory) =>
        Partials.TryGetValue(ResultingAssemblyProviderData.GetCacheFileHash(sourceLocation), out var resolvedSourceLocations)
     && resolvedSourceLocations.GetSourceLocation(assembly.MetadataName) is { } data
     && assembly.MatchesCachedVersion(data.CacheVersion)
            ? data
            : factory();

    public bool IsAssemblySkipped(IAssemblySymbol assembly) => SkipAssemblies.Contains(assembly.MetadataName);
}
