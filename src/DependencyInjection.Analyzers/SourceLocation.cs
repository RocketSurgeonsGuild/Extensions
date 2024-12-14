using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record SourceLocation
(
    [property: JsonPropertyName("l")]
    int LineNumber,
    string FilePath,
    [property: JsonPropertyName("a")]
    string ExpressionHash)
{
    [JsonIgnore]
    public string FileName => Path.GetFileName(FilePath);

    [JsonPropertyName("f")]
    public string FilePath { get; init; } = FilePath.Replace("\\", "/");
}

internal record ResolvedSourceLocation(SourceLocation Location, string Expression, [property: JsonIgnore] ImmutableHashSet<string> PrivateAssemblies);
