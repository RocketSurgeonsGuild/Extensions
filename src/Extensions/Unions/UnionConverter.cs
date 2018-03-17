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
            var obj = JToken.ReadFrom(reader);
            if (obj.Type == JTokenType.Null)
                return null;

            var context = GetContext(objectType);
            var instance = Activator.CreateInstance(context.GetTypeToDeserializeTo((JObject)obj));
            serializer.Populate(obj.CreateReader(), instance);
            return instance;
        }

        private UnionContext GetContext(Type type)
        {
            if (!_contexts.TryGetValue(type, out var context))
            {
                var rootType = UnionHelper.GetRootType(type);
                if (!_contexts.TryGetValue(rootType.AsType(), out var rootContext))
                {
                    rootContext = _contexts[rootType.AsType()] = new UnionContext(rootType);
                }
                context = _contexts[type] = rootContext;
            }

            return context;
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
