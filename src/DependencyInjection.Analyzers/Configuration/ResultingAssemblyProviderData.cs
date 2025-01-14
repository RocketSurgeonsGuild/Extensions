using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public class ResultingAssemblyProviderData
{
    public void AddAssemblyData(IAssemblySymbol assembly, CompiledAssemblyProviderData data)
    {
        if (_assemblyData.TryGetValue(assembly.MetadataName, out _)) return;

        _assemblyData.Add(assembly.MetadataName, data);
    }

    public bool AddSkipAssembly(IAssemblySymbol assembly) => _skipAssemblies.Add(assembly.MetadataName);

    public void AddSourceLocation(IAssemblySymbol assembly, ResolvedSourceLocation resolvedSource)
    {
        var cacheKey = GetCacheFileHash(resolvedSource.Location);
        if (!_sourceLocations.TryGetValue(cacheKey, out _)) _sourceLocations.Add(cacheKey, new(resolvedSource.Location));

        _sourceLocations[cacheKey].AddSource(assembly.MetadataName, resolvedSource);
    }

    public GeneratedAssemblyProviderData ToGeneratedAssemblyProviderData() => new(
        _assemblyData.ToImmutableDictionary(),
        [.. _skipAssemblies],
        _sourceLocations.ToImmutableDictionary(
            x => x.Key,
            x => new GeneratedLocationAssemblyResolvedSourceCollection(x.Value.SourceLocation, x.Value.ResolvedSources.ToImmutableDictionary())
        )
    );

    internal static string GetCacheFileHash(SourceLocation location)
    {
        using var hasher = MD5.Create();
        addStringToHash(hasher, location.FileName);
        addStringToHash(hasher, location.ExpressionHash);
        addStringToHash(hasher, location.LineNumber.ToString());
        _ = hasher.TransformFinalBlock([], 0, 0);
        return Convert.ToBase64String(hasher.Hash);

        static void addStringToHash(ICryptoTransform cryptoTransform, string textToHash)
        {
            var inputBuffer = Encoding.UTF8.GetBytes(textToHash);
            _ = cryptoTransform.TransformBlock(inputBuffer, 0, inputBuffer.Length, inputBuffer, 0);
        }
    }

    private readonly Dictionary<string, CompiledAssemblyProviderData> _assemblyData = [];
    private readonly HashSet<string> _skipAssemblies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ResultingLocationAssemblyResolvedSourceCollection> _sourceLocations = [];
}
