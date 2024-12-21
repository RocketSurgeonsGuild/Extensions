using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record CompiledAssemblyProviderData
(
    ImmutableList<ResolvedSourceLocation> AssemblySources,
    ImmutableList<string> InternalReflectionRequests,
    ImmutableList<string> InternalServiceDescriptorRequests,
    ImmutableHashSet<string> PrivateAssemblyNames
);
