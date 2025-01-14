using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class AssemblySymbolExtensions
{
    public static string? GetCachedVersion(this IAssemblySymbol assembly) => GetCompiledTypeProviderHash(assembly) is { Length: > 0 } hash
        ? hash
        : GetInformationalVersion(assembly);

    public static bool MatchesCachedVersion(this IAssemblySymbol assembly, string? cacheVersion) =>
        assembly.GetCachedVersion() is not { Length: > 0 } version || version == cacheVersion;

    private static string? GetCompiledTypeProviderHash(IAssemblySymbol assembly) =>
        assembly
          .GetAttributes()
          .FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == "Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute")
         ?.ConstructorArguments.LastOrDefault()
          .Value?.ToString();

    private static string? GetInformationalVersion(IAssemblySymbol assembly) =>
        assembly
          .GetAttributes()
          .FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == "System.Reflection.AssemblyInformationalVersionAttribute")
         ?.ConstructorArguments.FirstOrDefault()
          .Value?.ToString();
}
