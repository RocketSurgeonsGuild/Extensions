using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Binding;

/// <summary>
///     <see cref="IJsonBinder" /> is a way to bind complex objects, with nested keys.
/// </summary>
public interface IJsonBinder
{
    /// <summary>
    ///     Bind the values to the source type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">The values.</param>
    T Bind<T>(IEnumerable<KeyValuePair<string, string?>> values)
        where T : class, new();

    /// <summary>
    ///     Bind the values to the source type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">The values.</param>
    /// <param name="serializer">The serializer.</param>
    T Bind<T>(IEnumerable<KeyValuePair<string, string?>> values, JsonSerializer serializer)
        where T : class, new();

    /// <summary>
    ///     Bind the values to the source type
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="values">The values.</param>
    object Bind(Type objectType, IEnumerable<KeyValuePair<string, string?>> values);

    /// <summary>
    ///     Bind the values to the source type
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="values">The values.</param>
    /// <param name="serializer">The serializer.</param>
    object Bind(Type objectType, IEnumerable<KeyValuePair<string, string?>> values, JsonSerializer serializer);

    /// <summary>
    ///     Populate the values to the source type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <param name="values">The values.</param>
    T Populate<T>(T value, IEnumerable<KeyValuePair<string, string?>> values)
        where T : class;

    /// <summary>
    ///     Populate the values to the source type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <param name="values">The values.</param>
    /// <param name="serializer">The serializer.</param>
    T Populate<T>(T value, IEnumerable<KeyValuePair<string, string?>> values, JsonSerializer serializer)
        where T : class;

    /// <summary>
    ///     Parses the given key value pairs into a <see cref="JObject" />.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>Newtonsoft.Json.Linq.JObject.</returns>
    JObject Parse(IEnumerable<KeyValuePair<string, string?>> values);

    /// <summary>
    ///     Get a list of <see cref="JValue" />'s for a given object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    IEnumerable<KeyValuePair<string, JValue>> GetValues<T>(T value)
        where T : class;

    /// <summary>
    ///     Get a list of <see cref="JValue" />'s for a given object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The serializer.</param>
    IEnumerable<KeyValuePair<string, JValue>> GetValues<T>(T value, JsonSerializer serializer)
        where T : class;

    /// <summary>
    ///     Get a list of key value pairs for the given source object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    IEnumerable<KeyValuePair<string, string?>> From<T>(T value)
        where T : class;

    /// <summary>
    ///     Get a list of key value pairs for the given source object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The serializer.</param>
    IEnumerable<KeyValuePair<string, string?>> From<T>(T value, JsonSerializer serializer)
        where T : class;
}
