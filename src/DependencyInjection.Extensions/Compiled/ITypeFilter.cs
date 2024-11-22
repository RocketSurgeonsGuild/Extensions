namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     The Compiled Implementation Type Filter
/// </summary>
[PublicAPI]
public interface ITypeFilter
{
    /// <summary>
    ///     Will match all types that are assignable to <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type that should be assignable from the matching types.</typeparam>
    ITypeFilter AssignableTo<T>();

    /// <summary>
    ///     Will match all types that are assignable to the specified <paramref name="type" />.
    /// </summary>
    /// <param name="type">The type that should be assignable from the matching types.</param>
    ITypeFilter AssignableTo(Type type);

    /// <summary>
    ///     Will match all types that are assignable to any of the specified <paramref name="types" />.
    /// </summary>
    /// <param name="type">The first type that should be assignable from the matching types.</param>
    /// <param name="types">The types that should be assignable from the matching types.</param>
    ITypeFilter AssignableToAny(Type type, params Type[] types);

    /// <summary>
    ///     Will not match all types that are assignable to <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type that should be assignable from the matching types.</typeparam>
    ITypeFilter NotAssignableTo<T>();

    /// <summary>
    ///     Will not match all types that are assignable to the specified <paramref name="type" />.
    /// </summary>
    /// <param name="type">The type that should be assignable from the matching types.</param>
    ITypeFilter NotAssignableTo(Type type);

    /// <summary>
    ///     Will not match all types that are assignable to any of the specified <paramref name="types" />.
    /// </summary>
    /// <param name="type">The first type that should be assignable from the matching types.</param>
    /// <param name="types">The types that should be assignable from the matching types.</param>
    ITypeFilter NotAssignableToAny(Type type, params Type[] types);

    /// <summary>
    ///     Will match all types that end with
    /// </summary>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    ITypeFilter EndsWith(string value, params string[] values);

    /// <summary>
    ///     Will match all types that end with
    /// </summary>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    ITypeFilter NotEndsWith(string value, params string[] values);

    /// <summary>
    ///     Will match all types that start with
    /// </summary>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    ITypeFilter StartsWith(string value, params string[] values);

    /// <summary>
    ///     Will match all types that start with
    /// </summary>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    ITypeFilter NotStartsWith(string value, params string[] values);

    /// <summary>
    ///     Will match all types that contain the given values
    /// </summary>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    ITypeFilter Contains(string value, params string[] values);

    /// <summary>
    ///     Will match all types that contain the given values
    /// </summary>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    ITypeFilter NotContains(string value, params string[] values);

    /// <summary>
    ///     Will match all types in the exact same namespace as the type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The type in the namespace to include</typeparam>
    ITypeFilter InExactNamespaceOf<T>();

    /// <summary>
    ///     Will match all types in the exact same namespace as the type <paramref name="types" />
    /// </summary>
    /// <param name="type">The first type in the namespaces to include.</param>
    /// <param name="types">The types in the namespaces to include.</param>
    ITypeFilter InExactNamespaceOf(Type type, params Type[] types);

    /// <summary>
    ///     Will match all types in the exact same namespace as the type <paramref name="namespaces" />
    /// </summary>
    /// <param name="first">The first namespace to include.</param>
    /// <param name="namespaces">The namespace to include.</param>
    ITypeFilter InExactNamespaces(string first, params string[] namespaces);

    /// <summary>
    ///     Will match all types in the same namespace as the type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">A type inside the namespace to include.</typeparam>
    ITypeFilter InNamespaceOf<T>();

    /// <summary>
    ///     Will match all types in any of the namespaces of the <paramref name="types" /> specified.
    /// </summary>
    /// <param name="type">The first type in the namespaces to include.</param>
    /// <param name="types">The types in the namespaces to include.</param>
    ITypeFilter InNamespaceOf(Type type, params Type[] types);

    /// <summary>
    ///     Will match all types in any of the <paramref name="namespaces" /> specified.
    /// </summary>
    /// <param name="first">The first namespace to include.</param>
    /// <param name="namespaces">The namespaces to include.</param>
    ITypeFilter InNamespaces(string first, params string[] namespaces);

    /// <summary>
    ///     Will match all types outside of the same namespace as the type <typeparamref name="T" />.
    /// </summary>
    ITypeFilter NotInNamespaceOf<T>();

    /// <summary>
    ///     Will match all types outside of all of the namespaces of the <paramref name="types" /> specified.
    /// </summary>
    /// <param name="type">The first type in the namespaces to include.</param>
    /// <param name="types">The types in the namespaces to include.</param>
    ITypeFilter NotInNamespaceOf(Type type, params Type[] types);

    /// <summary>
    ///     Will match all types outside of all of the <paramref name="namespaces" /> specified.
    /// </summary>
    /// <param name="first">The first namespace to include.</param>
    /// <param name="namespaces">The namespaces to include.</param>
    ITypeFilter NotInNamespaces(string first, params string[] namespaces);

    /// <summary>
    ///     Will match all types that has an attribute of type <typeparamref name="T" /> defined.
    /// </summary>
    /// <typeparam name="T">The type of attribute that needs to be defined.</typeparam>
    ITypeFilter WithAttribute<T>() where T : Attribute;

    /// <summary>
    ///     Will match all types that has an attribute of <paramref name="attributeType" /> defined.
    /// </summary>
    /// <param name="attributeType">Type of the attribute.</param>
    ITypeFilter WithAttribute(Type attributeType);

    /// <summary>
    ///     Will match all types that has an attribute of <paramref name="attributeFullName" /> defined.
    /// </summary>
    /// <param name="attributeFullName">The full name of the attribute.</param>
    ITypeFilter WithAttribute(string? attributeFullName);

    /// <summary>
    ///     Will match all types that has an attribute of <paramref name="attributeType" /> defined.
    /// </summary>
    /// <param name="attributeType">Type of the attribute.</param>
    /// <param name="attributeTypes">Types of the attribute.</param>
    ITypeFilter WithAnyAttribute(Type attributeType, params Type[] attributeTypes);

    /// <summary>
    ///     Will match all types that has an attribute of <paramref name="attributeFullName" /> defined.
    /// </summary>
    /// <param name="attributeFullName">The full name of the attribute.</param>
    /// <param name="attributeFullNames">The full name of the attributes.</param>
    ITypeFilter WithAnyAttribute(string? attributeFullName, params string[] attributeFullNames);

    /// <summary>
    ///     Will match all types that doesn't have an attribute of type <typeparamref name="T" /> defined.
    /// </summary>
    /// <typeparam name="T">The type of attribute that needs to be defined.</typeparam>
    ITypeFilter WithoutAttribute<T>() where T : Attribute;

    /// <summary>
    ///     Will match all types that doesn't have an attribute of <paramref name="attributeType" /> defined.
    /// </summary>
    /// <param name="attributeType">Type of the attribute.</param>
    ITypeFilter WithoutAttribute(Type attributeType);

    /// <summary>
    ///     Will match all types that doesn't have an attribute of <paramref name="attributeFullName" /> defined.
    /// </summary>
    /// <param name="attributeFullName">The full name of the attribute.</param>
    ITypeFilter WithoutAttribute(string? attributeFullName);

    /// <summary>
    ///     Will match all types that are of the specified <paramref name="typeKindFilter" />.
    /// </summary>
    /// <param name="typeKindFilter"></param>
    /// <param name="typeKindFilters"></param>
    /// <returns></returns>
    ITypeFilter KindOf(TypeKindFilter typeKindFilter, params TypeKindFilter[] typeKindFilters);

    /// <summary>
    ///     Will match all types that are not of the specified <paramref name="typeKindFilter" />.
    /// </summary>
    /// <param name="typeKindFilter"></param>
    /// <param name="typeKindFilters"></param>
    /// <returns></returns>
    ITypeFilter NotKindOf(TypeKindFilter typeKindFilter, params TypeKindFilter[] typeKindFilters);

    /// <summary>
    ///     Will match all types that are of the specified <paramref name="typeInfoFilter" />.
    /// </summary>
    /// <param name="typeInfoFilter"></param>
    /// <param name="typeInfoFilters"></param>
    /// <returns></returns>
    ITypeFilter InfoOf(TypeInfoFilter typeInfoFilter, params TypeInfoFilter[] typeInfoFilters);

    /// <summary>
    ///     Will match all types that are not of the specified <paramref name="typeInfoFilter" />.
    /// </summary>
    /// <param name="typeInfoFilter"></param>
    /// <param name="typeInfoFilters"></param>
    /// <returns></returns>
    ITypeFilter NotInfoOf(TypeInfoFilter typeInfoFilter, params TypeInfoFilter[] typeInfoFilters);
}
