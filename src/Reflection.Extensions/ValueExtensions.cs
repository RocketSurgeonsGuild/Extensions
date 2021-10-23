using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection;

/// <summary>
///     Set backing value
/// </summary>
/// ary>
[PublicAPI]
public static class ValueExtension

/// <summary>
///     Sets the backing value.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TV"></typeparam>
/// <param name="instance">The instance.</param>
/// <param name="expression">The expression.</param>
/// <param name="value">The value.</param>
/// <returns></returns>returns>
public static T SetBackingValue<T, TV>(this T instance, Expression<Func<T, TV>> expression, TV value)
{
    BackingFieldHelper.Instance.SetBackingField(instance, expression, value);
    return instance;
}

}
