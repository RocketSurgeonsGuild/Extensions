using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    internal static class Traverse
    {
        public static IEnumerable<T> Across<T>(T first, Func<T, T> next)
            where T : class
        {
            var item = first;
            while (item != null)
            {
                yield return item;
                item = next(item);
            }
        }
    }

    internal static class TypeExtensions
    {
        public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
        {
            return TypesAssignableFrom(@this).Any(t => t.GetTypeInfo().IsGenericType && !@this.GetTypeInfo().ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric);
        }

        public static Type GetClosedTypeOf(this Type @this, Type openGeneric)
        {
            return TypesAssignableFrom(@this).FirstOrDefault(t => t.GetTypeInfo().IsGenericType && !@this.GetTypeInfo().ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric);
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
                Traverse.Across(candidateType, t => t.GetTypeInfo().BaseType));
        }
    }

}
