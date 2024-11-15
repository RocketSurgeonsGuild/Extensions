namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     The compiled assembly selector
/// </summary>
[PublicAPI]
public interface IReflectionAssemblySelector
{
    /// <summary>
    ///     Will scan for types from this assembly at compile time.
    /// </summary>
    IReflectionTypeSelector FromAssembly();

    /// <summary>
    ///     Will scan for types from all metadata assembly at compile time.
    /// </summary>
    IReflectionTypeSelector FromAssemblies();

    /// <summary>
    ///     Will load and scan from given types assembly
    /// </summary>
    IReflectionTypeSelector FromAssemblyDependenciesOf<T>();

    /// <summary>
    ///     Will load and scan from given types assembly
    /// </summary>
    IReflectionTypeSelector FromAssemblyDependenciesOf(Type type);

    /// <summary>
    ///     Will scan for types from the assembly of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type in which assembly that should be scanned.</typeparam>
    IReflectionTypeSelector FromAssemblyOf<T>();

    /// <summary>
    ///     Will scan for types from the assembly of type.
    /// </summary>
    IReflectionTypeSelector FromAssemblyOf(Type type);

    /// <summary>
    ///     Will not scan for types from the assembly of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type in which assembly that should be scanned.</typeparam>
    IReflectionTypeSelector NotFromAssemblyOf<T>();

    /// <summary>
    ///     Will not scan for types from the assembly of type.
    /// </summary>
    IReflectionTypeSelector NotFromAssemblyOf(Type type);

    /// <summary>
    ///     Include system assemblies
    /// </summary>
    /// <returns></returns>
    IReflectionTypeSelector IncludeSystemAssemblies();
}
