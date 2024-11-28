namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

/// <summary>
///     Enumeration for possible type information filters.
/// </summary>
internal enum TypeInfoFilter
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

    /// <summary>
    ///     The type is static.
    /// </summary>
    Static = -2,
}
