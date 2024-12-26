using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record ReflectionCollectionData
(
    [property: JsonPropertyName("t")]
    TypeFilterData Type
);
