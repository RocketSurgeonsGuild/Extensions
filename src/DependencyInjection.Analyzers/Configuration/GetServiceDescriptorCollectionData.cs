using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record GetServiceDescriptorCollectionData
(
    [property: JsonPropertyName("a")]
    AssemblyCollectionData Assembly,
    [property: JsonPropertyName("r")]
    ReflectionCollectionData Reflection,
    [property: JsonPropertyName("s")]
    ServiceDescriptorCollectionData ServiceDescriptor)
{
    [JsonPropertyName("t")]
    public string Type => nameof(GetServiceDescriptorCollectionData);
}
