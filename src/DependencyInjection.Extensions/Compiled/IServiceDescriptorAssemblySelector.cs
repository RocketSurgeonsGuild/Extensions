namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     The compiled assembly selector
/// </summary>
[PublicAPI]
public interface IServiceDescriptorAssemblySelector
{
    /// <summary>
    ///     Will scan for types from this assembly at compile time.
    /// </summary>
    IServiceDescriptorTypeSelector FromAssembly();

    /// <summary>
    ///     Will scan for types from all metadata assembly at compile time.
    /// </summary>
    IServiceDescriptorTypeSelector FromAssemblies();

    /// <summary>
    ///     Will load and scan from given types assembly
    /// </summary>
    IServiceDescriptorTypeSelector FromAssemblyDependenciesOf<T>();

    /// <summary>
    ///     Will load and scan from given types assembly
    /// </summary>
    IServiceDescriptorTypeSelector FromAssemblyDependenciesOf(Type type);

    /// <summary>
    ///     Will scan for types from the assembly of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type in which assembly that should be scanned.</typeparam>
    IServiceDescriptorTypeSelector FromAssemblyOf<T>();

    /// <summary>
    ///     Will scan for types from the assembly of type.
    /// </summary>
    IServiceDescriptorTypeSelector FromAssemblyOf(Type type);

    /// <summary>
    ///     Will not scan for types from the assembly of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type in which assembly that should be scanned.</typeparam>
    IServiceDescriptorTypeSelector NotFromAssemblyOf<T>();

    /// <summary>
    ///     Will not scan for types from the assembly of type.
    /// </summary>
    IServiceDescriptorTypeSelector NotFromAssemblyOf(Type type);

    /// <summary>
    ///     Include system assemblies
    /// </summary>
    /// <returns></returns>
    IServiceDescriptorTypeSelector IncludeSystemAssemblies();
}
