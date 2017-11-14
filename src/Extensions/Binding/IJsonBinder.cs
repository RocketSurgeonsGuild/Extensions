using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Interface IJsonBinder
    /// </summary>
    /// TODO Edit XML Comment Template for IJsonBinder
    public interface IJsonBinder
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>System.String.</returns>
        /// TODO Edit XML Comment Template for GetKey
        string GetKey(JToken token);
        /// <summary>
        /// Parses the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>JObject.</returns>
        /// TODO Edit XML Comment Template for Parse
        JObject Parse(IEnumerable<KeyValuePair<string, string>> values);

        /// <summary>
        /// Gets the specified values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <returns>T.</returns>
        /// TODO Edit XML Comment Template for Get`1
        T Get<T>(IEnumerable<KeyValuePair<string, string>> values) where T : class, new();
        
        /// <summary>
        /// Gets the specified values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>T.</returns>
        /// TODO Edit XML Comment Template for Get`1
        T Get<T>(IEnumerable<KeyValuePair<string, string>> values, JsonSerializer settings) where T : class, new();
    }
}
