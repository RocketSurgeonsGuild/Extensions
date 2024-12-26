using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record ServiceDescriptorCollectionData
(
    [property: JsonPropertyName("s")]
    ServiceDescriptorFilterData ServiceDescriptor,
    [property: JsonPropertyName("z")]
    int Lifetime
);
