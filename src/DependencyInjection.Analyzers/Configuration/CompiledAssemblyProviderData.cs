using System.Collections.Immutable;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record CompiledAssemblyProviderData
(
    ImmutableList<GetAssemblyConfiguration> InternalAssemblyRequests,
    ImmutableList<GetReflectionCollectionData> InternalReflectionRequests,
    ImmutableList<GetServiceDescriptorCollectionData> InternalServiceDescriptorRequests,
    bool ExcludeFromResolution,
    string? CacheVersion
)
{
    public bool IsEmpty =>
        InternalAssemblyRequests.IsEmpty
     && InternalReflectionRequests.IsEmpty
     && InternalServiceDescriptorRequests.IsEmpty;
}
