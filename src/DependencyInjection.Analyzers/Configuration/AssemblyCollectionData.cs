using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record AssemblyCollectionData
(
    [property: JsonPropertyName("l")]
    SourceLocation Location,
    [property: JsonPropertyName("a")]
    AssemblyFilterData Assembly
);