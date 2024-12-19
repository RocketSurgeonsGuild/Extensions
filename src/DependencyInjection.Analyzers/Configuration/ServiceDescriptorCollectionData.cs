using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record ServiceDescriptorCollectionData
(
    [property: JsonPropertyName("s")]
    ServiceDescriptorFilterData ServiceDescriptor,
    [property: JsonPropertyName("z")]
    int Lifetime
);