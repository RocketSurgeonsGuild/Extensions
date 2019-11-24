using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Unions
{
    /// <summary>
    /// Union Json Converter
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class UnionConverter : JsonConverter
    {
        private readonly IDictionary<Type, UnionContext> _contexts = new Dictionary<Type, UnionContext>();

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
                                        [NotNull] JsonSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            var obj = JToken.ReadFrom(reader);
            if (obj.Type == JTokenType.Null)
                return null!;

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
                if (rootType != null)
                {
                    if (!_contexts.TryGetValue(rootType.AsType(), out var rootContext))
                    {
                        rootContext = _contexts[rootType.AsType()] = new UnionContext(rootType);
                    }
                    context = _contexts[type] = rootContext;
                }
            }

            return context;
        }

        #region Base Class        
        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter" /> can read JSON.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="JsonConverter" /> can read JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter" /> can write JSON.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="JsonConverter" /> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite => false;

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType) => true;

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
        #endregion
    }
}
