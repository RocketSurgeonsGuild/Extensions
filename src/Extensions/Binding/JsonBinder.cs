using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Class JsonBinder.
    /// </summary>
    /// TODO Edit XML Comment Template for JsonBinder
    public class JsonBinder
    {
        private readonly string[] _separator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBinder"/> class.
        /// </summary>
        /// TODO Edit XML Comment Template for #ctor
        public JsonBinder() : this(':')
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBinder"/> class.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// TODO Edit XML Comment Template for #ctor
        public JsonBinder(char separator)
        {
            this._separator = new[] {separator.ToString()};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBinder"/> class.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// TODO Edit XML Comment Template for #ctor
        public JsonBinder(string separator)
        {
            this._separator = new[] { separator };
        }

        /// <summary>
        /// Gets the specified values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <returns>T.</returns>
        /// TODO Edit XML Comment Template for Get`1
        public T Get<T>(IEnumerable<KeyValuePair<string, string>> values)
        where T : class, new()
        {
            return Parse(values).ToObject<T>(GetSerializer(null));
        }

        /// <summary>
        /// Gets the specified values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>T.</returns>
        /// TODO Edit XML Comment Template for Get`1
        public T Get<T>(IEnumerable<KeyValuePair<string, string>> values, JsonSerializer serializer)
        where T : class, new()
        {
            return Parse(values).ToObject<T>(serializer);
        }


        /// <summary>
        /// Parses the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>Newtonsoft.Json.Linq.JObject.</returns>
        public JObject Parse(IEnumerable<KeyValuePair<string, string>> values)
        {
            var result = new JObject();
            foreach (var item in values)
            {
                var keys = item.Key.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                var prop = keys.Last();
                JToken root = result;

                // This produces a simple look ahead
                var zippedKeys = keys
                    .Zip(keys.Skip(1), (prev, current) => (prev: prev, current: current));

                foreach (var (key, next) in zippedKeys)
                {
                    if (int.TryParse(next, out var value))
                    {
                        root = SetValueToToken(root, key, new JArray());
                    }
                    else
                    {
                        root = SetValueToToken(root, key, new JObject());
                    }
                }

                SetValueToToken(root, prop, new JValue(item.Value));
            }
            return result;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>System.String.</returns>
        public string GetKey(JToken token)
        {
            var items = new Stack<string>();
            while (token.Parent != null)
            {
                if (token is JProperty p)
                {
                    items.Push(p.Name);
                }
                token = token.Parent;
            }
            return string.Join(_separator[0], items);
        }

        private T SetValueToToken<T>(JToken root, string key, T value)
        where T : JToken
        {
            if (GetValueFromToken(root, key) is null)
            {
                if (root is JArray arr)
                {
                    if (int.TryParse(key, out var index))
                    {
                        if (arr.Count <= index)
                            arr.Add(value);
                        else
                            arr[index] = value;

                        return value;
                    }
                }
                else
                {
                    root[key] = value;
                    return value;
                }
            }
            return (T)root[key];
        }

        private JToken GetValueFromToken(JToken root, string key)
        {
            if (root is JArray arr)
            {
                if (int.TryParse(key, out var index))
                {
                    if (arr.Count <= index) return null;
                    return arr[index];
                }
                throw new IndexOutOfRangeException(key);
            }
            return root[key];
        }

        private JsonSerializer GetSerializer(JsonSerializer serializer)
        {
            serializer = serializer ?? JsonSerializer.CreateDefault();

            if (!(serializer.ContractResolver is PrivateSetterContractResolver))
            {
                // ensure we have the correct settings to capture the backing field
                serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    CheckAdditionalContent = serializer.CheckAdditionalContent,
                    ConstructorHandling = serializer.ConstructorHandling,
                    Context = serializer.Context,
                    ContractResolver = new PrivateSetterContractResolver(),
                    Converters = serializer.Converters,
                    Culture = serializer.Culture,
                    DateFormatHandling = serializer.DateFormatHandling,
                    DateFormatString = serializer.DateFormatString,
                    DateParseHandling = serializer.DateParseHandling,
                    DateTimeZoneHandling = serializer.DateTimeZoneHandling,
                    DefaultValueHandling = serializer.DefaultValueHandling,
                    EqualityComparer = serializer.EqualityComparer,
                    FloatFormatHandling = serializer.FloatFormatHandling,
                    FloatParseHandling = serializer.FloatParseHandling,
                    Formatting = serializer.Formatting,
                    MaxDepth = serializer.MaxDepth,
                    MetadataPropertyHandling = serializer.MetadataPropertyHandling,
                    MissingMemberHandling = serializer.MissingMemberHandling,
                    NullValueHandling = serializer.NullValueHandling,
                    ObjectCreationHandling = serializer.ObjectCreationHandling,
                    PreserveReferencesHandling = serializer.PreserveReferencesHandling,
                    ReferenceLoopHandling = serializer.ReferenceLoopHandling,
                    StringEscapeHandling = serializer.StringEscapeHandling,
                    TypeNameAssemblyFormatHandling = serializer.TypeNameAssemblyFormatHandling,
                    TypeNameHandling = serializer.TypeNameHandling,
                    ReferenceResolverProvider = () => serializer.ReferenceResolver,
                    SerializationBinder = serializer.SerializationBinder,
                    TraceWriter = serializer.TraceWriter
                });
            }
            return serializer;
        }

    }
}
