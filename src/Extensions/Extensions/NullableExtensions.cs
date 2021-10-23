using System.Collections.ObjectModel;
using System.Reflection;

namespace Rocket.Surgery.Extensions;

/// <summary>
///     The nullability of a given property
/// </summary>
public enum Nullability
{
    /// <summary>
    ///     Not Defined
    /// </summary>
    NotDefined,

    /// <summary>
    ///     Nullable
    /// </summary>
    Nullable,

    /// <summary>
    ///     Non Nullable
    /// </summary>
    NonNullable
}

/// <summary>
///     Extensions for handling nullable types
/// </summary>
public static class NullableExtensions
{
    /// <summary>
    ///     Check if the given property is nullable or not
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public static Nullability GetNullability(this PropertyInfo property)
    {
        return NullabilityHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);
    }

    /// <summary>
    ///     Check if the given field is nullable or not
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static Nullability GetNullability(this FieldInfo field)
    {
        return NullabilityHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);
    }

    /// <summary>
    ///     Check if the given parameter is nullable or not
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static Nullability GetNullability(this ParameterInfo parameter)
    {
        return NullabilityHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);
    }

    /// <summary>
    ///     Check if the given parameter is nullable or not
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static Nullability GetNullability(this MethodInfo method)
    {
        return NullabilityHelper(
            method.ReturnType, method.DeclaringType, method.ReturnTypeCustomAttributes.GetCustomAttributes(false).OfType<CustomAttributeData>()
        );
    }

    private static Nullability NullabilityHelper(Type memberType, MemberInfo? declaringType, IEnumerable<CustomAttributeData> customAttributes)
    {
        if (memberType.IsValueType)
            return Nullable.GetUnderlyingType(memberType) == null ? Nullability.NonNullable : Nullability.Nullable;

        var nullable = customAttributes
           .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable?.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                {
                    return (byte)args[0].Value! == 2 ? Nullability.Nullable : Nullability.NonNullable;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value! == 2 ? Nullability.Nullable : Nullability.NonNullable;
            }
        }

        for (var type = declaringType; type != null; type = type.DeclaringType)
        {
            var context = type.CustomAttributes
                              .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context?.ConstructorArguments.Count == 1 &&
                context.ConstructorArguments[0].ArgumentType == typeof(byte))
            {
                return (byte)context.ConstructorArguments[0].Value! == 2 ? Nullability.Nullable : Nullability.NonNullable;
            }
        }

        // Couldn't find a suitable attribute
        return Nullability.NotDefined;
    }
}
