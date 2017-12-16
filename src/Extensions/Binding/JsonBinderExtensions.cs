using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Class JsonBinderExtensions.
    /// </summary>
    /// TODO Edit XML Comment Template for JsonBinderExtensions
    public static class JsonBinderExtensions
    {
        private static IEnumerable<KeyValuePair<string, string>> GetValues(IConfiguration configuration) =>
            configuration
            .AsEnumerable(true)
            .Where(x => x.Value != null);

        /// <summary>
        /// Bind the values to the given configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binder">The binder.</param>
        /// <param name="configuration">The configuration.</param>
        public static T Bind<T>(this IJsonBinder binder, IConfiguration configuration)
            where T : class, new()
        {
            return binder.Bind<T>(GetValues(configuration));
        }

        /// <summary>
        /// Bind the values to the given configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binder">The binder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        public static T Bind<T>(this IJsonBinder binder, IConfiguration configuration, JsonSerializer serializer)
            where T : class, new()
        {
            return binder.Bind<T>(GetValues(configuration), serializer);
        }

        /// <summary>
        /// Bind the values to the given configuration
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="objectType"></param>
        /// <param name="configuration">The configuration.</param>
        public static object Bind(this IJsonBinder binder, Type objectType, IConfiguration configuration)
        {
            return binder.Bind(objectType, GetValues(configuration));
        }

        /// <summary>
        /// Bind the values to the given configuration
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="objectType"></param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        public static object Bind(this IJsonBinder binder, Type objectType, IConfiguration configuration, JsonSerializer serializer)
        {
            return binder.Bind(objectType, GetValues(configuration), serializer);
        }

        /// <summary>
        /// Populate the values from the given configuration object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binder">The binder.</param>
        /// <param name="value">The value.</param>
        /// <param name="configuration">The configuration.</param>
        public static T Populate<T>(this IJsonBinder binder, T value, IConfiguration configuration)
            where T : class
        {
            return binder.Populate(value, GetValues(configuration));
        }

        /// <summary>
        /// Populate the values from the given configuration object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binder">The binder.</param>
        /// <param name="value">The value.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        public static T Populate<T>(this IJsonBinder binder, T value, IConfiguration configuration, JsonSerializer serializer)
            where T : class
        {
            return binder.Populate(value, GetValues(configuration), serializer);
        }
    }
}
