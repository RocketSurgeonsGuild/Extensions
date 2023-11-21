using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning disable CA1307 // Specify StringComparison
namespace Rocket.Surgery.Reflection;

/// <summary>
///     The default backing field helper
/// </summary>
public class BackingFieldHelper
{
    /// <summary>
    ///     A default instance
    /// </summary>
    public static BackingFieldHelper Instance { get; } = new BackingFieldHelper();

    private readonly ConcurrentDictionary<(Type, string), FieldInfo> _backingFields = new ConcurrentDictionary<(Type, string), FieldInfo>();

    private static FieldInfo? GetBackingField(PropertyInfo? pi)
    {
        if (pi == null || !pi.CanRead || pi.GetMethod?.IsDefined(typeof(CompilerGeneratedAttribute), true) != true)
            return null;

        var backingField = pi.DeclaringType?.GetTypeInfo().GetDeclaredField($"<{pi.Name}>k__BackingField");
        if (backingField == null)
            return null;

        if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), true))
            return null;

        return backingField;
    }

    private FieldInfo GetBackingField(Type objectType, Type interfaceType, string name)
    {
        if (!_backingFields.TryGetValue((objectType, name), out var backingField))
        {
            var property = objectType.GetTypeInfo().GetProperty(
                $"{interfaceType.FullName?.Replace("+", ".")}.{name}", BindingFlags.NonPublic | BindingFlags.Instance
            ) ?? objectType.GetTypeInfo().GetProperty(name);

            backingField = GetBackingField(property)!;
            _backingFields.TryAdd((objectType, name), backingField);
        }

        if (backingField is null)
            throw new NotSupportedException("Given Expression is not supported");
        return backingField;
    }

    /// <summary>
    ///     Gets the backing field.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="type">The type.</param>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public FieldInfo GetBackingField<TInterface, TValue>(Type type, Expression<Func<TInterface, TValue>> expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        if (expression.Body is MemberExpression exp)
        {
            return GetBackingField(type, typeof(TInterface), exp.Member.Name);
        }

        throw new NotSupportedException("Given Expression is not supported");
    }

    /// <summary>
    ///     Sets the backing field.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="value">The value.</param>
    public void SetBackingField<TInterface, TValue>(TInterface instance, Expression<Func<TInterface, TValue>> expression, TValue value)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        if (expression.Body is MemberExpression)
        {
            var field = GetBackingField(instance!.GetType(), expression);
            field.SetValue(instance, value);
            return;
        }

        throw new NotSupportedException("Given Expression is not supported");
    }
}
