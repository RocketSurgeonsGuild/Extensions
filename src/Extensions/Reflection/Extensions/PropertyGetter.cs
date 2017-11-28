using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class PropertyGetter
    {
        private readonly string _separator;
        private readonly ConcurrentDictionary<(Type type, string path), (Type type, Lazy<Delegate> getter)> CachedPropertyGetters = new ConcurrentDictionary<(Type type, string path), (Type type, Lazy<Delegate> getter)>();
        public PropertyGetter(string separator = null)
        {
            _separator = separator;
        }

        public T Get<T>(object value, string path)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Getter<T>(value.GetType(), path)(value);
        }

        public object Get(object value, string path)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Getter(value.GetType(), path)(value);
        }

        public Type GetPropertyType(object value, string path)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return GetPropertyType(value.GetType(), path);
        }

        public Func<object, T> Getter<T>(Type value, string path)
        {
            if (!CachedPropertyGetters.TryGetValue((value, path), out var tuple))
            {
                tuple = CreateGetter(value, path);
                CachedPropertyGetters.TryAdd((value, path), tuple);
            }

            return v => (T)tuple.getter.Value.DynamicInvoke(v);
        }

        public Func<object, object> Getter(Type value, string path)
        {
            if (!CachedPropertyGetters.TryGetValue((value, path), out var tuple))
            {
                tuple = CreateGetter(value, path);
                CachedPropertyGetters.TryAdd((value, path), tuple);
            }

            return v => tuple.getter.Value.DynamicInvoke(v);
        }

        public Type GetPropertyType(Type value, string path)
        {
            if (!CachedPropertyGetters.TryGetValue((value, path), out var tuple))
            {
                tuple = CreateGetter(value, path);
                CachedPropertyGetters.TryAdd((value, path), tuple);
            }
            return tuple.type;
        }

        private string[] GetParts(string path)
        {
            var parts = new List<string>();
            if (string.IsNullOrWhiteSpace(_separator))
            {
                var current = new StringBuilder();
                foreach (var c in path)
                {
                    if (c == '.' || c == '[' || c == ']')
                    {
                        parts.Add(current.ToString());
                        current = new StringBuilder();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                parts.Add(current.ToString());
                return parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            }

            return path.Split(new[] { _separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        private (Type type, Lazy<Delegate> getter) CreateGetter(Type type, string path)
        {
            var parts = GetParts(path);
            var root = Expression.Parameter(type, "instance");
            Expression expression = root;
            var innerType = type;

            foreach (var part in parts)
            {
                if (innerType.IsArray)
                {
                    if (!int.TryParse(part, out var intValue))
                    {
                        throw new ArgumentException($"Could not parse integer value for indexer from '{part}'.");
                    }
                    expression = Expression.ArrayIndex(expression, Expression.Constant(intValue, typeof(int)));
                    innerType = innerType.GetTypeInfo().GetElementType();
                    continue;
                }
                if (innerType.IsGenericListInterfaceType())
                {
                    if (!int.TryParse(part, out var intValue))
                    {
                        throw new ArgumentException($"Could not parse integer value for indexer from '{part}'.");
                    }

                    var paremeter = innerType.GetRuntimeProperties()
                        .First(x => x.GetIndexParameters().Any(z => z.ParameterType == typeof(int)));

                    expression = Expression.MakeIndex(expression, paremeter, new[] { Expression.Constant(intValue, typeof(int)) });
                    innerType = innerType.GetGenericListInterfaceType().GenericTypeArguments[0];
                    continue;
                }
                if (innerType.IsClosedTypeOf(typeof(IDictionary<,>)) || innerType.IsClosedTypeOf(typeof(IReadOnlyDictionary<,>)))
                {
                    var paremeter = innerType.GetRuntimeProperties()
                        .First(x => x.GetIndexParameters().Any(z => z.ParameterType == typeof(string)));

                    expression = Expression.MakeIndex(expression, paremeter, new[] { Expression.Constant(part, typeof(string)) });
                    innerType = (innerType.GetClosedTypeOf(typeof(IDictionary<,>)) ?? innerType.GetClosedTypeOf(typeof(IReadOnlyDictionary<,>))).GenericTypeArguments[1];
                    continue;
                }
                if (innerType.IsGenericEnumerableInterfaceType())
                {
                    innerType = innerType.GetGenericEnumerableInterfaceType().GenericTypeArguments[0];
                    var firstMethod = typeof(Enumerable).GetRuntimeMethods().First(x =>
                            x.Name == nameof(Enumerable.FirstOrDefault) && x.GetParameters().Length == 1)
                        .MakeGenericMethod(innerType);
                    var skipMethod = typeof(Enumerable).GetRuntimeMethods().First(x =>
                            x.Name == nameof(Enumerable.Skip))
                        .MakeGenericMethod(innerType);

                    if (!int.TryParse(part, out var intValue))
                    {
                        throw new ArgumentException($"Could not parse integer value for indexer from '{part}'.");
                    }

                    expression = Expression.Call(
                        null, skipMethod, expression, Expression.Constant(intValue, typeof(int))
                    );
                    expression = Expression.Call(null, firstMethod, expression);
                    continue;
                }

                var info = Info.GetInfo(innerType, part);
                innerType = info.Type;
                expression = Expression.PropertyOrField(expression, info.Name);
            }

            return (innerType, new Lazy<Delegate>(() => Expression.Lambda(expression, root).Compile()));
        }

        class Info
        {
            public static Info GetInfo(Type type, string name)
            {
                var propertyInfo = type.GetRuntimeProperty(name);
                if (propertyInfo != null)
                {
                    return new Info(propertyInfo.PropertyType, name);
                }

                var fieldInfo = type.GetRuntimeField(name);
                if (fieldInfo != null)
                {
                    return new Info(fieldInfo.FieldType, name);
                }

                throw new ArgumentOutOfRangeException("path", $"Could not find property or field '{name}'.");
            }

            public Info(Type type, string name)
            {
                Name = name;
                Type = type;
            }

            public string Name { get; }
            public Type Type { get; }
        }


    }
}
