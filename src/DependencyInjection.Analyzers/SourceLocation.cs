using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record SourceLocation
(
    [property: JsonPropertyName("l")]
    int LineNumber,
    [property: JsonPropertyName("f")]
    string FilePath,
    [property: JsonPropertyName("a")]
    string ExpressionHash)
{
    public string FileName => Path.GetFileName(FilePath);
}