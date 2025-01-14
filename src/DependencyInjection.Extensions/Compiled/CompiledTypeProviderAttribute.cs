namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
/// Attribute used to define the compiled type provider for a given assembly
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class CompiledTypeProviderAttribute(Type type, string generatedHash) : Attribute
{
    /// <summary>
    /// The generated hash to be used for cache busting
    /// </summary>
    public string GeneratedHash { get; } = generatedHash;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private Lazy<ICompiledTypeProvider> _compiledTypeProvider = new(() => (ICompiledTypeProvider)Activator.CreateInstance(type)!);

    /// <summary>
    /// The assembly provider
    /// </summary>
    public ICompiledTypeProvider ICompiledTypeProvider => _compiledTypeProvider.Value;

    /// <summary>
    /// The type
    /// </summary>
    public Type Type => type;
}
