using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class PropertyGetter
    {
        private readonly string _separator;
        private readonly StringComparison _comparison;
        private readonly ConcurrentDictionary<Type, TypeDelegate> _cachedTypeDelegates = new ConcurrentDictionary<Type, TypeDelegate>();
        public PropertyGetter(string separator = null, StringComparison comparison = StringComparison.Ordinal)
        {
            _separator = separator;
            _comparison = comparison;
        }

        public bool TryGet<T>(object instance, string path, out T value, bool throwError = false)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (TryGetter<T>(instance.GetType(), path, out var getter))
            {
                value = getter(instance);
                return true;
            }

            value = default;
            return false;
        }

        public T Get<T>(object instance, string path)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (TryGet<T>(instance, path, out var propertyValue)) return propertyValue;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGet(object instance, string path, out object value, bool throwError = false)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            if (TryGetter(instance.GetType(), path, out var getter))
            {
                value = getter(instance);
                return true;
            }

            value = null;
            return false;
        }

        public object Get(object instance, string path)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (TryGet(instance, path, out var propertyValue)) return propertyValue;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetPropertyType(object instance, string path, out Type type)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return TryGetPropertyType(instance.GetType(), path, out type);
        }

        public Type GetPropertyType(object instance, string path)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (TryGetPropertyType(instance, path, out var propertyValue)) return propertyValue;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetPropertyType(Type type, string path, out Type propertyType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var typeDelegate = GetOrCreateTypeDelegate(type);
            if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
            {
                propertyType = null;
                return false;
            }

            propertyType = propertyDelegate.PropertyType;
            return true;
        }

        public Type GetPropertyType(Type type, string path)
        {
            if (TryGetPropertyType(type, path, out var propertyValue)) return propertyValue;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetter<T>(Type type, string path, out Func<object, T> getter)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var typeDelegate = GetOrCreateTypeDelegate(type);
            if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
            {
                getter = null;
                return false;
            }

            getter = v => (T)propertyDelegate.Delegate.DynamicInvoke(v);
            return true;
        }

        public Func<object, T> Getter<T>(Type type, string path)
        {
            if (TryGetter<T>(type, path, out var getter)) return getter;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetter(Type type, string path, out Func<object, object> getter)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var typeDelegate = GetOrCreateTypeDelegate(type);
            if (!typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
            {
                getter = null;
                return false;
            }

            getter = v => propertyDelegate.Delegate.DynamicInvoke(v);
            return true;
        }

        public Func<object, object> Getter(Type type, string path)
        {
            if (TryGetter(type, path, out var getter)) return getter;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetExpression(object instance, string path, out Expression expression)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return TryGetExpression(instance.GetType(), path, out expression);
        }

        public Expression GetExpression(object instance, string path)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (TryGetExpression(instance, path, out var expression)) return expression;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetExpression(Type type, string path, out Expression expression)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var typeDelegate = GetOrCreateTypeDelegate(type);
            if (typeDelegate.TryGetPropertyDelegate(path, out var propertyDelegate))
            {
                expression = propertyDelegate.StronglyTypedExpression;
                return true;
            }

            expression = null;
            return false;
        }

        public Expression GetExpression(Type type, string path)
        {
            if (TryGetExpression(type, path, out var expression)) return expression;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetPropertyDelegate(object instance, string path, out PropertyDelegate propertyDelegate)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return TryGetPropertyDelegate(instance.GetType(), path, out propertyDelegate);
        }

        public PropertyDelegate GetPropertyDelegate(object instance, string path)
        {
            if (TryGetPropertyDelegate(instance, path, out var propertyDelegate)) return propertyDelegate;
            throw new ArgumentOutOfRangeException(nameof(path), $"Could not find property or field '{path}'.");
        }

        public bool TryGetPropertyDelegate(Type type, string path, out PropertyDelegate propertyDelegate)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var typeDelegate = GetOrCreateTypeDelegate(type);
            return typeDelegate.TryGetPropertyDelegate(path, out propertyDelegate);
        }

        public PropertyDelegate GetPropertyDelegate(Type type, string path)
        {
            if (TryGetPropertyDelegate(type, path, out var propertyDelegate)) return propertyDelegate;
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
}
