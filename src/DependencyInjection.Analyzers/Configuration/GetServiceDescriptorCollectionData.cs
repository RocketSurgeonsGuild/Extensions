using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record GetServiceDescriptorCollectionData
(
    [property: JsonPropertyName("a")]
    AssemblyCollectionData Assembly,
    [property: JsonPropertyName("r")]
    ReflectionCollectionData Reflection,
    [property: JsonPropertyName("s")]
    ServiceDescriptorCollectionData ServiceDescriptor)
{
    public string Type => nameof(GetServiceDescriptorCollectionData);
}