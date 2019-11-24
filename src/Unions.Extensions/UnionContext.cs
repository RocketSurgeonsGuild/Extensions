using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Unions
{
    internal class UnionContext
    {
        private readonly IReadOnlyDictionary<object, Type> _enumTypeList;

        private readonly string _camelCasePropertyName;
        private readonly string _pascalCasePropertyName;
        private readonly string _dashCasePropertyName;
        private readonly Type _enumType;
        private readonly string _propertyName;

        public UnionContext(TypeInfo rootType)
        {
            var unionKeyAttribute = rootType.GetCustomAttribute<UnionKeyAttribute>();
            _propertyName = unionKeyAttribute.Key;
            _camelCasePropertyName = Inflector.Camelize(_propertyName);
            _pascalCasePropertyName = Inflector.Pascalize(_propertyName);
            _dashCasePropertyName = Inflector.Kebaberize(_propertyName);
            _enumType = UnionHelper.GetUnionEnumType(rootType);
            _enumTypeList = UnionHelper.GetUnion(rootType);
        }

        public Type GetTypeToDeserializeTo(JObject obj)
        {
            var property =
                obj[_camelCasePropertyName] ??
                obj[_pascalCasePropertyName] ??
                obj[_dashCasePropertyName] ??
                throw new KeyNotFoundException($"Could not find property name for {_propertyName}");
            var value = property.Value<string>();
            var result = Enum.Parse(_enumType, value, true);
            return _enumTypeList[result];
        }
    }
}
