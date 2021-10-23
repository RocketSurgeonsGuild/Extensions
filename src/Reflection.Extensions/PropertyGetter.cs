using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection;

/// <summary>
///     Property getter
/// </summary>
public class PropertyGetter
{
    private readonly string? _separator;
    private readonly StringComparison _comparison;
    private readonly ConcurrentDictionary<Type, TypeDelegate> _cachedTypeDelegates = new ConcurrentDictionary<Type, TypeDelegate>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyGetter" /> class.
    /// </summary>
    /// <param name="separator">The separator.</param>
    /// <param name="comparison">The comparison.</param>
    public PropertyGetter(string? separator = null, StringComparison comparison = StringComparison.Ordinal)
    {
        _separator = separator;
        _comparison = comparison;
    }

    /// <summary>
    ///     Tries the get.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool TryGet<T>(object instance, string path, out T value)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (path == null) throw new ArgumentNullException(nameof(path));

        if (TryGetter<T>(instance.GetType(), path, out var getter))
        {
            value = getter!(instance);
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    ///     Gets the specified instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public T Get<T>(object instance, string path)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (TryGet<T>(instance, path, out var propertyValue)) return propertyValue;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Tries the get.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool TryGet(object instance, string path, out object value)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        if (TryGetter(instance.GetType(), path, out var getter))
        {
            value = getter!(instance);
            return true;
        }

        value = null!;
        return false;
    }

    /// <summary>
    ///     Gets the specified instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public object Get(object instance, string path)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (TryGet(instance, path, out var propertyValue)) return propertyValue!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Tries the type of the get property.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public bool TryGetPropertyType(object instance, string path, out Type type)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return TryGetPropertyType(instance.GetType(), path, out type);
    }

    /// <summary>
    ///     Gets the type of the property.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Type GetPropertyType(object instance, string path)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (TryGetPropertyType(instance, path, out var propertyValue)) return propertyValue!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Tries the type of the get property.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="propertyType">Type of the property.</param>
    /// <returns></returns>
    public bool TryGetPropertyType(Type type, string path, out Type propertyType)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (path == null) throw new ArgumentNullException(nameof(path));

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
    ///     Gets the type of the property.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Type GetPropertyType(Type type, string path)
    {
        if (TryGetPropertyType(type, path, out var propertyValue)) return propertyValue!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Tries the getter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="getter">The getter.</param>
    /// <returns></returns>
    public bool TryGetter<T>(Type type, string path, out Func<object, T> getter)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (path == null) throw new ArgumentNullException(nameof(path));

        var typeDelegate = GetOrCreateTypeDelegate(type);
        if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
        {
            getter = null!;
            return false;
        }

        getter = v => (T)propertyDelegate.Delegate.DynamicInvoke(v);
        return true;
    }

    /// <summary>
    ///     Getters the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Func<object, T> Getter<T>(Type type, string path)
    {
        if (TryGetter<T>(type, path, out var getter)) return getter!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    /// <summary>
    ///     Tries the getter.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <param name="getter">The getter.</param>
    /// <returns></returns>
    public bool TryGetter(Type type, string path, out Func<object, object> getter)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (path == null) throw new ArgumentNullException(nameof(path));

        var typeDelegate = GetOrCreateTypeDelegate(type);
        if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
        {
            getter = null!;
            return false;
        }

        getter = v => propertyDelegate.Delegate.DynamicInvoke(v);
        return true;
    }

    /// <summary>
    ///     Getters the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Func<object, object> Getter(Type type, string path)
    {
        if (TryGetter(type, path, out var getter)) return getter;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
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
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return TryGetExpression(instance.GetType(), path, out expression);
    }

    /// <summary>
    ///     Gets the expression.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Expression GetExpression(object instance, string path)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (TryGetExpression(instance, path, out var expression)) return expression!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
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
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (path == null) throw new ArgumentNullException(nameof(path));

        var typeDelegate = GetOrCreateTypeDelegate(type);
        if (typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
        {
            expression = propertyDelegate!.StronglyTypedExpression;
            return true;
        }

        expression = null!;
        return false;
    }

    /// <summary>
    ///     Gets the expression.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public Expression GetExpression(Type type, string path)
    {
        if (TryGetExpression(type, path, out var expression)) return expression!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
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
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return TryGetPropertyDelegate(instance.GetType(), path, out propertyDelegate);
    }

    /// <summary>
    ///     Gets the property delegate.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public PropertyDelegate GetPropertyDelegate(object instance, string path)
    {
        if (TryGetPropertyDelegate(instance, path, out var propertyDelegate)) return propertyDelegate!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
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
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (path == null) throw new ArgumentNullException(nameof(path));

        var typeDelegate = GetOrCreateTypeDelegate(type);
        return typeDelegate.TryGetPropertyDelegate(path, out propertyDelegate);
    }

    /// <summary>
    ///     Gets the property delegate.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public PropertyDelegate GetPropertyDelegate(Type type, string path)
    {
        if (TryGetPropertyDelegate(type, path, out var propertyDelegate)) return propertyDelegate!;
        throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
    }

    private TypeDelegate GetOrCreateTypeDelegate(Type type)
    {
        if (!_cachedTypeDelegates.TryGetValue(type, out var typeDelegate))
        {
            _cachedTypeDelegates.TryAdd(type, typeDelegate = new TypeDelegate(type, _separator, _comparison));
        }

        return typeDelegate;
    }
}
