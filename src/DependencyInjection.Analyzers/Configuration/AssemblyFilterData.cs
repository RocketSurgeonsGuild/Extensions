using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record AssemblyFilterData
(
    [property: JsonPropertyName("a")]
    bool AllAssembly,
    [property: JsonPropertyName("i")]
    bool IncludeSystem,
    [property: JsonPropertyName("m")]
    ImmutableArray<string> Assembly,
    [property: JsonPropertyName("na")]
    ImmutableArray<string> NotAssembly,
    [property: JsonPropertyName("d")]
    ImmutableArray<string> AssemblyDependencies
);
