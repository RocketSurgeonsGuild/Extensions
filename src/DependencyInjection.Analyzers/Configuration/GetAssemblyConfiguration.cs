using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record GetAssemblyConfiguration
(
    [property: JsonPropertyName("a")]
    AssemblyCollectionData Assembly
)
{
    public string Type => nameof(GetAssemblyConfiguration);
};