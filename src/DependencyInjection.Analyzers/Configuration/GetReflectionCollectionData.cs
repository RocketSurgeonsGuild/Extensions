using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record GetReflectionCollectionData
(
    [property: JsonPropertyName("a")]
    AssemblyCollectionData Assembly,
    [property: JsonPropertyName("r")]
    ReflectionCollectionData Reflection
)
{
    [JsonPropertyName("t")]
    public string Type => nameof(GetReflectionCollectionData);
}
