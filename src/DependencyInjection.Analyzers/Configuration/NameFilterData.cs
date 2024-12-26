using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record NameFilterData
(
    [property: JsonPropertyName("i")]
    bool Include,
    [property: JsonPropertyName("f")]
    TextDirectionFilter Filter,
    [property: JsonPropertyName("n")]
    ImmutableArray<string> Names);
