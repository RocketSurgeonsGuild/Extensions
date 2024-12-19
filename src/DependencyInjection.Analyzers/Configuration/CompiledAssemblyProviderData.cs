using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

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