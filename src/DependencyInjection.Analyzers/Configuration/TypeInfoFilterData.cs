using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record TypeInfoFilterData
(
    [property: JsonPropertyName("f")]
    bool Include,
    [property: JsonPropertyName("t")]
    ImmutableArray<TypeInfoFilter> TypeInfos);