using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record ReflectionCollectionData
(
    [property: JsonPropertyName("t")]
    TypeFilterData Type
);