using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record CompiledAssemblyProviderData
(
    ImmutableList<GetAssemblyConfiguration> InternalAssemblyRequests,
    ImmutableList<GetReflectionCollectionData> InternalReflectionRequests,
    ImmutableList<GetServiceDescriptorCollectionData> InternalServiceDescriptorRequests
)
{
    public bool IsEmpty =>
        InternalAssemblyRequests.IsEmpty
     && InternalReflectionRequests.IsEmpty
     && InternalServiceDescriptorRequests.IsEmpty;
}
