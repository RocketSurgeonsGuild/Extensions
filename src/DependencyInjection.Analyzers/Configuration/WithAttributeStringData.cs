using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record WithAttributeStringData
(
    [property: JsonPropertyName("i")]
    bool Include,
    [property: JsonPropertyName("b")]
    string Attribute);