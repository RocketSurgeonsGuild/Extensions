namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     The Compiled Implementation Type Selector
/// </summary>
[PublicAPI]
public interface IServiceDescriptorTypeSelector : IServiceDescriptorAssemblySelector
{
    /// <summary>
    ///     Adds all public, non-abstract classes from the selected assemblies to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
    /// </summary>
    IServiceTypeSelector AddClasses();

    /// <summary>
    ///     Adds all non-abstract classes from the selected assemblies to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
    /// </summary>
    /// <param name="publicOnly">Specifies whether too add public types only.</param>
    IServiceTypeSelector AddClasses(bool publicOnly);

    /// <summary>
    ///     Adds all public, non-abstract classes from the selected assemblies that
    ///     matches the requirements specified in the <paramref name="action" />
    ///     to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
    /// </summary>
    /// <param name="action">The filtering action.</param>
    /// <exception cref="ArgumentNullException">If the <paramref name="action" /> argument is <c>null</c>.</exception>
    IServiceTypeSelector AddClasses(Action<ITypeFilter> action);

    /// <summary>
    ///     Adds all non-abstract classes from the selected assemblies that
    ///     matches the requirements specified in the <paramref name="action" />
    ///     to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
    /// </summary>
    /// <param name="action">The filtering action.</param>
    /// <exception cref="ArgumentNullException">If the <paramref name="action" /> argument is <c>null</c>.</exception>
    /// <param name="publicOnly">Specifies whether too add public types only.</param>
    IServiceTypeSelector AddClasses(Action<ITypeFilter> action, bool publicOnly);
}
