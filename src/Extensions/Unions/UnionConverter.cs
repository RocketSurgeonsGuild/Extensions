using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Unions
{
    public class UnionConverter : JsonConverter
    {
        private readonly IDictionary<Type, UnionContext> _contexts = new Dictionary<Type, UnionContext>();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = (JObject)JToken.ReadFrom(reader);
            var context = GetContext(objectType);
            var instance = Activator.CreateInstance(context.GetTypeToDeserializeTo(obj));
            serializer.Populate(obj.CreateReader(), instance);
            return instance;
        }

        private UnionContext GetContext(Type type)
        {
            if (!_contexts.TryGetValue(type, out var context))
            {
                context = _contexts[type] = new UnionContext(type.GetTypeInfo());
            }

            return context;
        }

        class UnionContext
        {
            private readonly IReadOnlyDictionary<object, Type> _enumTypeList;

            private readonly string _camelCasePropertyName;
            private readonly string _pascalCasePropertyName;
            private readonly string _dashCasePropertyName;
            private readonly Type _enumType;
            private readonly string _propertyName;

            public UnionContext(TypeInfo typeInfo)
            {
                var rootType = typeInfo;
                while (typeInfo.BaseType != null)
                {
                    if (typeInfo.GetCustomAttributes<UnionKeyAttribute>().Any()) break;
                    rootType = rootType.BaseType.GetTypeInfo();
                }

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

        #region Base Class
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
