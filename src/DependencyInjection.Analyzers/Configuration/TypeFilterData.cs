using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record TypeFilterData
(
    [property: JsonPropertyName("b")]
    ClassFilter Filter,
    [property: JsonPropertyName("c")]
    ImmutableArray<NamespaceFilterData> NamespaceFilters,
    [property: JsonPropertyName("d")]
    ImmutableArray<NameFilterData> NameFilters,
    [property: JsonPropertyName("e")]
    ImmutableArray<TypeKindFilterData> TypeKindFilters,
    [property: JsonPropertyName("f")]
    ImmutableArray<TypeInfoFilterData> TypeInfoFilters,
    [property: JsonPropertyName("g")]
    ImmutableArray<WithAttributeData> WithAttributeFilters,
    [property: JsonPropertyName("h")]
    ImmutableArray<WithAttributeStringData> WithAttributeStringFilters,
    [property: JsonPropertyName("i")]
    ImmutableArray<WithAttributeData> WithAnyAttributeFilters,
    [property: JsonPropertyName("j")]
    ImmutableArray<WithAttributeStringData> WithAnyAttributeStringFilters,
    [property: JsonPropertyName("k")]
    ImmutableArray<AssignableToTypeData> AssignableToTypeFilters,
    [property: JsonPropertyName("l")]
    ImmutableArray<AssignableToAnyTypeData> AssignableToAnyTypeFilters
);
