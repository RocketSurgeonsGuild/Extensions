using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record AssemblyCollectionData
(
    [property: JsonPropertyName("l")]
    SourceLocation Location,
    [property: JsonPropertyName("a")]
    AssemblyFilterData Assembly
);
