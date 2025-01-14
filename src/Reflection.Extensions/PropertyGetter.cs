#if NETSTANDARD2_0
#pragma warning disable CS8602, CS8603
#else
using System.Collections.Concurrent;
#endif
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection;

/// <summary>
///     Property getter
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="PropertyGetter" /> class.
/// </remarks>
/// <param name="separator">The separator.</param>
/// <param name="comparison">The comparison.</param>
[PublicAPI]
public class PropertyGetter(string? separator = null, StringComparison comparison = StringComparison.Ordinal)
{
    /// <summary>
    ///     Gets the specified instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public T Get<T>(object instance, string path)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return TryGet<T>(instance, path, out var propertyValue)
            ? propertyValue
            : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Gets the specified instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public object Get(object instance, string path)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return TryGet(instance, path, out var propertyValue)
            ? propertyValue
            : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Gets the expression.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Expression GetExpression(object instance, string path)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return TryGetExpression(instance, path, out var expression)
            ? expression
            : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Gets the expression.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Expression GetExpression(Type type, string path) => TryGetExpression(type, path, out var expression)
        ? expression
        : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");

    /// <summary>
    ///     Gets the property delegate.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public PropertyDelegate GetPropertyDelegate(object instance, string path) => TryGetPropertyDelegate(instance, path, out var propertyDelegate)
        ? propertyDelegate
        : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");

    /// <summary>
    ///     Gets the property delegate.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public PropertyDelegate GetPropertyDelegate(Type type, string path) => TryGetPropertyDelegate(type, path, out var propertyDelegate)
        ? propertyDelegate
        : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");

    /// <summary>
    ///     Gets the type of the property.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Type GetPropertyType(object instance, string path)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return TryGetPropertyType(instance, path, out var propertyValue)
            ? propertyValue
            : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Gets the type of the property.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Type GetPropertyType(Type type, string path) => TryGetPropertyType(type, path, out var propertyValue)
        ? propertyValue
        : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");

    /// <summary>
    ///     Getters the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Func<object, T> Getter<T>(Type type, string path) => TryGetter<T>(type, path, out var getter)
        ? getter
        : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");

    /// <summary>
    ///     Getters the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Func<object, object> Getter(Type type, string path) => TryGetter(type, path, out var getter)
        ? getter
        : throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");

    /// <summary>
    ///     Tries the get.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool TryGet<T>(
        object instance,
        string path,
#if !NETSTANDARD2_0
        [NotNullWhen(true)]
        #endif
        out T? value
    )
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(path);

        if (TryGetter<T>(instance.GetType(), path, out var getter))
        {
            value = getter(instance)!;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    ///     Tries the get.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool TryGet(
        object instance,
        string path,
#if !NETSTANDARD2_0
        [NotNullWhen(true)]
        #endif
        out object? value
    )
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (TryGetter(instance.GetType(), path, out var getter))
        {
            value = getter(instance);
            return true;
        }

        value = null!;
        return false;
    }

    /// <summary>
    ///     Tries the get expression.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public bool TryGetExpression(object instance, string path, out Expression expression)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return TryGetExpression(instance.GetType(), path, out expression);
    }

    /// <summary>
    ///     Tries the get expression.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public bool TryGetExpression(Type type, string path, out Expression expression)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(path);

        var typeDelegate = GetOrCreateTypeDelegate(type);
        if (typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
        {
            expression = propertyDelegate.StronglyTypedExpression;
            return true;
        }

        expression = null!;
        return false;
    }

    /// <summary>
    ///     Tries the get property delegate.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="propertyDelegate">The property delegate.</param>
    /// <returns></returns>
    public bool TryGetPropertyDelegate(object instance, string path, out PropertyDelegate propertyDelegate)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return TryGetPropertyDelegate(instance.GetType(), path, out propertyDelegate);
    }

    /// <summary>
    ///     Tries the get property delegate.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="propertyDelegate">The property delegate.</param>
    /// <returns></returns>
    public bool TryGetPropertyDelegate(Type type, string path, out PropertyDelegate propertyDelegate)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(path);

        var typeDelegate = GetOrCreateTypeDelegate(type);
        return typeDelegate.TryGetPropertyDelegate(path, out propertyDelegate);
    }

    /// <summary>
    ///     Tries the type of the get property.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public bool TryGetPropertyType(
        object instance,
        string path,
#if !NETSTANDARD2_0
        [NotNullWhen(true)]
        #endif
        out Type? type
    )
    {
        ArgumentNullException.ThrowIfNull(instance);
        return TryGetPropertyType(instance.GetType(), path, out type);
    }

    /// <summary>
    ///     Tries the type of the get property.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="propertyType">Type of the property.</param>
    /// <returns></returns>
    public bool TryGetPropertyType(
        Type type,
        string path,
#if !NETSTANDARD2_0
        [NotNullWhen(true)]
        #endif
        out Type? propertyType
    )
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(path);

        var typeDelegate = GetOrCreateTypeDelegate(type);
        if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
        {
            propertyType = null!;
            return false;
        }

        propertyType = propertyDelegate.PropertyType;
        return true;
    }

    /// <summary>
    ///     Tries the getter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="getter">The getter.</param>
    /// <returns></returns>
    public bool TryGetter<T>(
        Type type,
        string path,
#if !NETSTANDARD2_0
        [NotNullWhen(true)]
        #endif
        out Func<object, T>? getter
    )
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(path);

        var typeDelegate = GetOrCreateTypeDelegate(type);
        if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
        {
            getter = null!;
            return false;
        }

        getter = v => (T)propertyDelegate.Delegate.DynamicInvoke(v)!;
        return true;
    }

    /// <summary>
    ///     Tries the getter.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="getter">The getter.</param>
    /// <returns></returns>
    public bool TryGetter(
        Type type,
        string path,
#if !NETSTANDARD2_0
        [NotNullWhen(true)]
        #endif
        out Func<object, object>? getter
    )
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(path);

        var typeDelegate = GetOrCreateTypeDelegate(type);
        if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
        {
            getter = null!;
            return false;
        }

        getter = v => propertyDelegate.Delegate.DynamicInvoke(v)!;
        return true;
    }

    private TypeDelegate GetOrCreateTypeDelegate(Type type)
    {
        if (!_cachedTypeDelegates.TryGetValue(type, out var typeDelegate)) _cachedTypeDelegates.TryAdd(type, typeDelegate = new(type, _separator, _comparison));

        return typeDelegate;
    }

    private readonly ConcurrentDictionary<Type, TypeDelegate> _cachedTypeDelegates = new();
    private readonly StringComparison _comparison = comparison;
    private readonly string? _separator = separator;
}
