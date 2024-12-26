using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record AssignableToAnyTypeData
(
    [property: JsonPropertyName("i")]
    bool Include,
    [property: JsonPropertyName("t")]
    ImmutableArray<AnyTypeData> Types);
