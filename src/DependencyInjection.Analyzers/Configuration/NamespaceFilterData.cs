using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record NamespaceFilterData
(
    [property: JsonPropertyName("f")]
    NamespaceFilter Filter,
    [property: JsonPropertyName("n")]
    ImmutableArray<string> Namespaces);