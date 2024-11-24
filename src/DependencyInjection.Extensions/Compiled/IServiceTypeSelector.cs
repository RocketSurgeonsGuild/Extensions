namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     The Compiled Service Type Selector
/// </summary>
[PublicAPI]
public interface IServiceTypeSelector
{
    /// <summary>
    ///     Registers each matching concrete type as itself.
    /// </summary>
    IServiceLifetimeSelector AsSelf();

    /// <summary>
    ///     Registers each matching concrete type as <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type to register as.</typeparam>
    IServiceLifetimeSelector As<T>();

    /// <summary>
    ///     Registers each matching concrete type as
    ///     <param name="type">type</param>
    /// </summary>
    IServiceLifetimeSelector As(Type type);

    /// <summary>
    ///     Registers each matching concrete type as all of its implemented interfaces.
    /// </summary>
    IServiceLifetimeSelector AsImplementedInterfaces();

    /// <summary>
    ///     Registers each matching concrete type as all of its implemented interfaces.
    /// </summary>
    IServiceLifetimeSelector AsImplementedInterfaces(Action<ITypeFilter> action);

    /// <summary>
    ///     Registers each matching concrete type as all of its implemented interfaces, by returning an instance of the main type
    /// </summary>
    IServiceLifetimeSelector AsSelfWithInterfaces();

    /// <summary>
    ///     Registers each matching concrete type as all of its implemented interfaces, by returning an instance of the main type
    /// </summary>
    IServiceLifetimeSelector AsSelfWithInterfaces(Action<ITypeFilter> action);

    /// <summary>
    ///     Registers the type with the first found matching interface name.  (e.g. ClassName is matched to IClassName)
    /// </summary>
    IServiceLifetimeSelector AsMatchingInterface();
}
