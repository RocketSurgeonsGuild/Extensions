using System.Reflection;

namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
/// Assembly provider extensions
/// </summary>
public static class CompiledTypeProviderExtensions
{
    /// <summary>
    /// Get the assembly provider for the given assembly
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ICompiledTypeProvider GetCompiledTypeProvider(this Assembly? assembly) =>
        assembly?.GetCustomAttribute<CompiledTypeProviderAttribute>()?.ICompiledTypeProvider
     ?? throw new InvalidOperationException("No CompiledTypeProviderAttribute found on the assembly");
}