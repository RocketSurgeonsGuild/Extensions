using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record CompiledAssemblyProviderData
(
    ImmutableList<ResolvedSourceLocation> AssemblySources,
    ImmutableList<GetReflectionCollectionData> InternalReflectionRequests,
    ImmutableList<GetServiceDescriptorCollectionData> InternalServiceDescriptorRequests,
    ImmutableHashSet<string> PrivateAssemblyNames
)
{
    public bool IsEmpty =>
        AssemblySources.IsEmpty
     && InternalReflectionRequests.IsEmpty
     && InternalServiceDescriptorRequests.IsEmpty
     && PrivateAssemblyNames.IsEmpty;
}
