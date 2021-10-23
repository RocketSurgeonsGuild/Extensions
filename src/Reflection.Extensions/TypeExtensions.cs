using System.Collections.ObjectModel;
using System.Reflection;

namespace Rocket.Surgery.Reflection;

internal static class TypeExtensions
{
    public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
    {
        return TypesAssignableFrom(@this).Any(
            t => t.GetTypeInfo().IsGenericType && !@this.GetTypeInfo().ContainsGenericParameters && t.Get
        enericTypeDefinition() == openGeneric
            );
    }

    public static Type GetClosedTypeOf(this Type @this, Type openGeneric)
    {
        return TypesAssignableFrom(@this).FirstOrDefault
        (
            t => t.GetTypeInfo().IsGenericType && !@this.GetTypeInfo().ContainsGenericParameters && t.GetGenericTypeDefinition()
             == openGeneric
        );
    }

    public static bool IsGenericEnumerableInterfaceType(this Type type)
    {
        return type.IsGenericTypeDefinedBy(typeof(IEnumerable<>))
            || type.IsGenericListInterfaceType()
            || type.IsGenericCollectionInterfaceType();
    }

    public static Type GetGenericEnumerableInterfaceType(this Type type)
    {
        return type.GetGenericListInterfaceType() ??
               type.GetGenericCollectionInterfaceType() ??
               type.GetClosedTypeOf(typeof(IEnumerable<>));
    }

    public static bool IsGenericCollectionInterfaceType(this Type type)
    {
        return type.IsGenericTypeDefinedBy(typeof(ICollection<>))
            || type.IsGenericTypeDefinedBy(typeof(Collection<>))
            || type.IsGenericTypeDefinedBy(typeof(IReadOnlyCollection<>))
            || type.IsGenericTypeDefinedBy(typeof(ReadOnlyCollection<>));
    }

    public static Type GetGenericCollectionInterfaceType(this Type type)
    {
        return type.GetClosedTypeOf(typeof(ICollection<>)) ?? type.GetClosedTypeOf(typeof(IReadOnlyCollection<>));
    }

    public static bool IsGenericListInterfaceType(this Type type)
    {
        return type.IsGenericTypeDefinedBy(typeof(IList<>))
            || type.IsGenericTypeDefinedBy(typeof(List<>))
            || type.IsGenericTypeDefinedBy(typeof(IReadOnlyList<>));
    }

    public static Type GetGenericListInterfaceType(this Type type)
    {
        return type.GetClosedTypeOf(typeof(IList<>)) ?? type.GetClosedTypeOf(typeof(IReadOnlyList<>));
    }

    public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
    {
        return !@this.GetTypeInfo().ContainsGenericParameters
            && @this.GetTypeInfo().IsGenericType
            && @this.GetGenericTypeDefinition() == openGeneric;
    }

    public static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
    {
        return candidateType.GetTypeInfo().ImplementedInterfaces.Concat(
            Traverse.Across(candidateType, t =>
                                t.GetTypeInf
        o().BaseType)
            );
    }
}
