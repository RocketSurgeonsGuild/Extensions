using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rocket.Surgery.Reflection.Extensions
{
    class TypeDelegate : IEquatable<TypeDelegate?>
    {
        private readonly string _separator;
        private readonly StringComparison _comparison;
        private readonly bool _shouldThrow;
        private readonly ConcurrentDictionary<string, PropertyDelegate> _propertyGetters = new ConcurrentDictionary<string, PropertyDelegate>();

        public TypeDelegate(Type type, string separator, StringComparison comparison, bool shouldThrow = false)
        {
            _separator = separator;
            _comparison = comparison;
            _shouldThrow = shouldThrow;
            Type = type;
        }

        public Type Type { get; }

        public IEnumerable<PropertyDelegate> PropertyGetters => _propertyGetters.Values;

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

        public bool TryGetPropertyDelegate(string path, out PropertyDelegate propertyDelegate)
        {
            var parts = GetParts(path);
            var root = Expression.Parameter(Type, "instance");
            Expression expression = root;
            var innerType = Type;

            foreach (var part in parts)
            {
                if (innerType.IsArray)
                {
                    if (!int.TryParse(part, out var intValue))
                    {
                        if (_shouldThrow) throw new Exception($"Could not parse integer value for indexer from '{part}'.");
                        propertyDelegate = null!;
                        return false;
                    }
                    expression = Expression.ArrayIndex(expression, Expression.Constant(intValue, typeof(int)));
                    innerType = innerType.GetTypeInfo().GetElementType();
                    continue;
                }
                if (innerType.IsGenericListInterfaceType())
                {
                    if (!int.TryParse(part, out var intValue))
                    {
                        if (_shouldThrow) throw new Exception($"Could not parse integer value for indexer from '{part}'.");
                        propertyDelegate = null!;
                        return false;
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
                        if (_shouldThrow) throw new Exception($"Could not parse integer value for indexer from '{part}'.");
                        propertyDelegate = null!;
                        return false;
                    }

                    expression = Expression.Call(
                        null, skipMethod, expression, Expression.Constant(intValue, typeof(int))
                    );
                    expression = Expression.Call(null, firstMethod, expression);
                    continue;
                }

                if (!Info.TryGetInfo(innerType, part, _comparison, out var info))
                {
                    if (_shouldThrow) throw new Exception($"Could not find property or field '{part}'.");
                    propertyDelegate = null!;
                    return false;
                }
                innerType = info!.Type;
                expression = Expression.PropertyOrField(expression, info.Name);
            }

            propertyDelegate = new PropertyDelegate(this, path, innerType, expression, root);
            _propertyGetters.TryAdd(path, propertyDelegate);
            return true;
        }

        class Info
        {
            public static bool TryGetInfo(Type type, string name, StringComparison comparison, out Info info)
            {
                var propertyInfo = type.GetRuntimeProperties()
                    .FirstOrDefault(x => x.Name.Equals(name, comparison));
                if (propertyInfo != null)
                {
                    info = new Info(propertyInfo.PropertyType, propertyInfo.Name);
                    return true;
                }

                var fieldInfo = type.GetRuntimeFields()
                    .FirstOrDefault(x => x.Name.Equals(name, comparison));
                if (fieldInfo != null)
                {
                    info = new Info(fieldInfo.FieldType, fieldInfo.Name);
                    return true;
                }

                info = null!;
                return false;
            }

            public Info(Type type, string name)
            {
                Name = name;
                Type = type;
            }

            public string Name { get; }
            public Type Type { get; }
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TypeDelegate);
        }

        public bool Equals(TypeDelegate? other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override int GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public static bool operator ==(TypeDelegate? delegate1, TypeDelegate? delegate2)
        {
            return EqualityComparer<TypeDelegate>.Default.Equals(delegate1!, delegate2!);
        }

        public static bool operator !=(TypeDelegate? delegate1, TypeDelegate? delegate2)
        {
            return !(delegate1 == delegate2);
        }
    }
}
