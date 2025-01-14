using System.Collections.Immutable;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record GeneratedLocationAssemblyResolvedSourceCollection(SourceLocation SourceLocation, ImmutableDictionary<string, ResolvedSourceLocation> ResolvedSources)
{
    public ResolvedSourceLocation? GetSourceLocation(string assemblyName) =>
        ResolvedSources.TryGetValue(assemblyName, out var resolvedSourceLocation)
            ? resolvedSourceLocation
            : null;
}
