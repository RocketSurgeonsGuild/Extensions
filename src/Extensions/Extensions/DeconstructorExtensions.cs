

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Default deconstructor for key value pairs
    /// </summary>
    public static class DeconstructorExtensions
    {
        /// <summary>
        /// Deconstructs the specified KVP.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="kvp">The KVP.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> kvp, out K key, out V value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
