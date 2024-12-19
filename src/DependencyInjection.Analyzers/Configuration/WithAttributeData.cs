using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record WithAttributeData
(
    [property: JsonPropertyName("i")]
    bool Include,
    [property: JsonPropertyName("a")]
    string Assembly,
    [property: JsonPropertyName("b")]
    string Attribute,
    [property: JsonPropertyName("u")]
    bool UnboundGenericType);