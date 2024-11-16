namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     Enumeration for possible type information filters.
/// </summary>
public enum TypeInfoFilter
{
    /// <summary>
    ///     The type is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     The type is abstract.
    /// </summary>
    Abstract = 1,

    /// <summary>
    ///     The type is visible.
    /// </summary>
    Visible = 2,

    /// <summary>
    ///     The type is a value type.
    /// </summary>
    ValueType = 3,

//    /// <summary>
//    ///   The type is nested.
//    /// </summary>
//    Nested = 4,

    /// <summary>
    ///     The type is sealed.
    /// </summary>
    Sealed = 5,

    /// <summary>
    ///     The type is a generic type.
    /// </summary>
    GenericType = 6,

//    /// <summary>
//    ///   The type is a generic type definition.
//    /// </summary>
//    GenericTypeDefinition = 7,
}