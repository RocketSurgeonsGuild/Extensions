namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     The Compiled Implementation Type Selector
/// </summary>
[PublicAPI]
public interface IReflectionTypeSelector : IReflectionAssemblySelector
{
    /// <summary>
    ///     Lists all of the classes in a given assembly
    /// </summary>
    IEnumerable<Type> GetTypes();

    /// <summary>
    ///     Lists all of the public classes in a given assembly
    /// </summary>
    /// <param name="publicOnly">Specifies whether too add public types only.</param>
    IEnumerable<Type> GetTypes(bool publicOnly);

    /// <summary>
    ///     Adds all public, non-abstract classes from the selected assemblies that
    ///     matches the requirements specified in the <paramref name="action" />
    ///     to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
    /// </summary>
    /// <param name="action">The filtering action.</param>
    IEnumerable<Type> GetTypes(Action<ITypeFilter> action);

    /// <summary>
    ///     Adds all non-abstract classes from the selected assemblies that
    ///     matches the requirements specified in the <paramref name="action" />
    ///     to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
    /// </summary>
    /// <param name="action">The filtering action.</param>
    /// <param name="publicOnly">Specifies whether too add public types only.</param>
    IEnumerable<Type> GetTypes(bool publicOnly, Action<ITypeFilter> action);
}
