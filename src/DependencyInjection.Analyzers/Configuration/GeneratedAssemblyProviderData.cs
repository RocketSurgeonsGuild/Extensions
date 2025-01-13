using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public class ResultingAssemblyProviderData
{
    private readonly Dictionary<string, CompiledAssemblyProviderData> _assemblyData = new();
    private readonly HashSet<string> _skipAssemblies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ResultingLocationAssemblyResolvedSourceCollection> _sourceLocations = new();

    public void AddSourceLocation(string assemblyName, ResolvedSourceLocation resolvedSource)
    {
        var cacheKey = GetCacheFileHash(resolvedSource.Location);
        if (!_sourceLocations.TryGetValue(cacheKey, out var value)) _sourceLocations.Add(cacheKey, new(resolvedSource.Location));
        _sourceLocations[cacheKey].AddSource(assemblyName, resolvedSource);
    }

    public void AddAssemblyData(string assemblyName, CompiledAssemblyProviderData data)
    {
        if (_assemblyData.TryGetValue(assemblyName, out _)) return;
        _assemblyData.Add(assemblyName, data);
    }

    public void AddSkipAssembly(string assemblyName)
    {
        _skipAssemblies.Add(assemblyName);
    }

    internal static string GetCacheFileHash(SourceLocation location)
    {
        using var hasher = MD5.Create();
        addStringToHash(hasher, location.FileName);
        addStringToHash(hasher, location.ExpressionHash);
        addStringToHash(hasher, location.LineNumber.ToString());
        var hash = hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return hasher.Hash.Aggregate("", (s, b) => s + b.ToString("x2"));


        static void addStringToHash(ICryptoTransform cryptoTransform, string textToHash)
        {
            var inputBuffer = Encoding.UTF8.GetBytes(textToHash);
            cryptoTransform.TransformBlock(inputBuffer, 0, inputBuffer.Length, inputBuffer, 0);
        }
    }

    public GeneratedAssemblyProviderData ToGeneratedAssemblyProviderData()
    {
        return new GeneratedAssemblyProviderData(
            _assemblyData.ToImmutableDictionary(),
            _skipAssemblies.ToImmutableHashSet(),
            _sourceLocations.ToImmutableDictionary(
                x => x.Key,
                x => new GeneratedLocationAssemblyResolvedSourceCollection(x.Value.SourceLocation, x.Value.ResolvedSources.ToImmutableDictionary())
            )
        );
    }
};

public record GeneratedAssemblyProviderData
(
    ImmutableDictionary<string, CompiledAssemblyProviderData> AssemblyData,
    ImmutableHashSet<string> SkipAssemblies,
    ImmutableDictionary<string, GeneratedLocationAssemblyResolvedSourceCollection> Partials
)
{
    public ResolvedSourceLocation? GetSourceLocation(string assemblyName, SourceLocation sourceLocation, Func<ResolvedSourceLocation?> factory)
    {
        var cacheKey = ResultingAssemblyProviderData.GetCacheFileHash(sourceLocation);
        return Partials.TryGetValue(cacheKey, out var resolvedSourceLocations)
         && resolvedSourceLocations.GetSourceLocation(assemblyName) is { } resolvedSourceLocation
                ? resolvedSourceLocation
                : factory();
    }
}

public record ResultingLocationAssemblyResolvedSourceCollection(SourceLocation SourceLocation)
{
    public Dictionary<string, ResolvedSourceLocation> ResolvedSources { get; } = new();

    public void AddSource(string assemblyName, ResolvedSourceLocation resolvedSource)
    {
        ResolvedSources[assemblyName] = resolvedSource;
    }
}

public record GeneratedLocationAssemblyResolvedSourceCollection(SourceLocation SourceLocation, ImmutableDictionary<string, ResolvedSourceLocation> ResolvedSources)
{
    public ResolvedSourceLocation? GetSourceLocation(string assemblyName)
    {
        return ResolvedSources.TryGetValue(assemblyName, out var resolvedSourceLocation)
            ? resolvedSourceLocation
            : null;
    }
}
