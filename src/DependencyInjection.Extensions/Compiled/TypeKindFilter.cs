namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     Enumeration for possible kinds of type symbols.
/// </summary>
public enum TypeKindFilter : byte
{
    /// <summary>
    ///     Type is an array type.
    /// </summary>
    Array = 1,

    /// <summary>
    ///     Type is a class.
    /// </summary>
    Class = 2,

    /// <summary>
    ///     Type is a delegate.
    /// </summary>
    Delegate = 3,

    /// <summary>
    ///     Type is an enumeration.
    /// </summary>
    Enum = 5,

    /// <summary>
    ///     Type is an interface.
    /// </summary>
    Interface = 7,

    /// <summary>
    ///     Type is a C# struct or VB Structure
    /// </summary>
    Struct = 10,
}