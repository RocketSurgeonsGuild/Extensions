using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Class JsonBinderExtensions.
    /// </summary>
    /// TODO Edit XML Comment Template for JsonBinderExtensions
    public static class JsonBinderExtensions
    {
        /// <summary>
        /// Gets the specified configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binder">The binder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>T.</returns>
        /// TODO Edit XML Comment Template for Get`1
        public static T Get<T>(this IJsonBinder binder, IEnumerable<KeyValuePair<string, string>> configuration)
        where T : class, new()
        {
            return binder.Get<T>(configuration, JsonSerializer.CreateDefault());
        }

        /// <summary>
        /// Gets the specified configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binder">The binder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>T.</returns>
        /// TODO Edit XML Comment Template for Get`1
        public static T Get<T>(this IJsonBinder binder, IEnumerable<KeyValuePair<string, string>> configuration, JsonSerializer serializer)
        where T : class, new()
        {
            return binder.Get<T>(configuration, serializer);
        }
    }
}
