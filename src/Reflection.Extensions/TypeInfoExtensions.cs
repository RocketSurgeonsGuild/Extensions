using System.Reflection;

namespace Rocket.Surgery.Reflection;

/// <summary>
///     TypeInfoExtensions.
/// </summary>
/// TODO Edit XML Comment Template for TypeInfoExtensionsions
public static class TypeInfoExtension

/// <summary>
///     Finds the declared property.
/// </summary>
/// <param name="typeInfo">The type information.</param>
/// <param name="name">The name.</param>
/// <returns>PropertyInfo.</returns>
/// TODO Edit XML Comment Template for FindDeclaredPropertyProperty
public static PropertyInfo? FindDeclaredProperty(this TypeInfo? typeInfo, string name)
{
    while (typeInfo != null)
    {
        var property = typeInfo.GetDeclaredProperty(name);
        if (property != null)
            return property;

        typeInfo = typeInfo?.BaseType?.GetTypeInfo();
    }

    return null;
}

}
