using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record SourceLocation
(
    [property: JsonPropertyName("l")]
    int LineNumber,
    string FilePath,
    [property: JsonPropertyName("e")]
    string ExpressionHash)
{
    [JsonIgnore]
    public string FileName => Path.GetFileName(FilePath);

    [JsonPropertyName("f")]
    public string FilePath { get; init; } = FilePath.Replace("\\", "/");
}

public record ResolvedSourceLocation(SourceLocation Location, string Expression, [property: JsonIgnore] ImmutableHashSet<string> PrivateAssemblies);

public record SavedSourceLocation(
    [property: JsonPropertyName("e")]
    string Expression,
    [property: JsonPropertyName("a")]
    ImmutableHashSet<string> PrivateAssemblies);
