using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

#pragma warning disable CA2201
namespace Rocket.Surgery.Reflection;

internal class TypeDelegate : IEquatable<TypeDelegate?>
{
    public static bool operator ==(TypeDelegate? delegate1, TypeDelegate? delegate2) => EqualityComparer<TypeDelegate>.Default.Equals(delegate1!, delegate2!);

    public static bool operator !=(TypeDelegate? delegate1, TypeDelegate? delegate2) => !( delegate1 == delegate2 );
    private readonly string? _separator;
    private readonly StringComparison _comparison;
    private readonly bool _shouldThrow;
    private readonly ConcurrentDictionary<string, PropertyDelegate> _propertyGetters = new();

    public TypeDelegate(Type type, string? separator, StringComparison comparison, bool shouldThrow = false)
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
                    current = new();
                }
                else
                {
                    current.Append(c);
                }
            }

            parts.Add(current.ToString());
            return parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }

        return path.Split([_separator], StringSplitOptions.RemoveEmptyEntries);
    }

    public bool TryGetPropertyDelegate(string path, out PropertyDelegate propertyDelegate)
    {
        var parts = GetParts(path);
        var root = Expression.Parameter(Type, "instance");
        Expression expression = root;
        var innerType = Type;

        foreach (var part in parts)
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            if (innerType!.IsArray)
            {
                if (!int.TryParse(part, out var intValue))
                {
                    if (_shouldThrow) throw new($"Could not parse integer value for indexer from '{part}'.");
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
                    if (_shouldThrow) throw new($"Could not parse integer value for indexer from '{part}'.");
                    propertyDelegate = null!;
                    return false;
                }

                var paremeter = innerType
                               .GetRuntimeProperties()
                               .First(x => x.GetIndexParameters().Any(z => z.ParameterType == typeof(int)));

                expression = Expression.MakeIndex(expression, paremeter, [Expression.Constant(intValue, typeof(int))]);
                innerType = innerType.GetGenericListInterfaceType()!.GenericTypeArguments[0];
                continue;
            }

            if (innerType.IsClosedTypeOf(typeof(IDictionary<,>)) || innerType.IsClosedTypeOf(typeof(IReadOnlyDictionary<,>)))
            {
                var paremeter = innerType
                               .GetRuntimeProperties()
                               .First(x => x.GetIndexParameters().Any(z => z.ParameterType == typeof(string)));

                expression = Expression.MakeIndex(expression, paremeter, [Expression.Constant(part, typeof(string))]);
                innerType = ( innerType.GetClosedTypeOf(typeof(IDictionary<,>)) ?? innerType.GetClosedTypeOf(typeof(IReadOnlyDictionary<,>)) )
                  ?.GenericTypeArguments[1];
                continue;
            }

            if (innerType.IsGenericEnumerableInterfaceType())
            {
                innerType = innerType.GetGenericEnumerableInterfaceType()!.GenericTypeArguments[0];
                var firstMethod = typeof(Enumerable)
                                 .GetRuntimeMethods()
                                 .First(
                                      x =>
                                          x.Name == nameof(Enumerable.FirstOrDefault) && x.GetParameters().Length == 1
                                  )
                                 .MakeGenericMethod(innerType);
                var skipMethod = typeof(Enumerable)
                                .GetRuntimeMethods()
                                .First(
                                     x =>
                                         x.Name == nameof(Enumerable.Skip)
                                 )
                                .MakeGenericMethod(innerType);

                if (!int.TryParse(part, out var intValue))
                {
                    if (_shouldThrow) throw new($"Could not parse integer value for indexer from '{part}'.");
                    propertyDelegate = null!;
                    return false;
                }

                expression = Expression.Call(
                    null,
                    skipMethod,
                    expression,
                    Expression.Constant(intValue, typeof(int))
                );
                expression = Expression.Call(null, firstMethod, expression);
                continue;
            }

            if (!Info.TryGetInfo(innerType, part, _comparison, out var info))
            {
                if (_shouldThrow) throw new($"Could not find property or field '{part}'.");
                propertyDelegate = null!;
                return false;
            }

            innerType = info.Type;
            expression = Expression.PropertyOrField(expression, info.Name);
        }

        propertyDelegate = new(this, path, innerType!, expression, root);
        _propertyGetters.TryAdd(path, propertyDelegate);
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as TypeDelegate);

    public override int GetHashCode() => 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);

    public bool Equals(TypeDelegate? other) =>
        other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);

    private class Info
    {
        public static bool TryGetInfo(Type type, string name, StringComparison comparison, out Info info)
        {
            var propertyInfo = type
                              .GetRuntimeProperties()
                              .FirstOrDefault(x => x.Name.Equals(name, comparison));
            if (propertyInfo != null)
            {
                info = new(propertyInfo.PropertyType, propertyInfo.Name);
                return true;
            }

            var fieldInfo = type
                           .GetRuntimeFields()
                           .FirstOrDefault(x => x.Name.Equals(name, comparison));
            if (fieldInfo != null)
            {
                info = new(fieldInfo.FieldType, fieldInfo.Name);
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
}
