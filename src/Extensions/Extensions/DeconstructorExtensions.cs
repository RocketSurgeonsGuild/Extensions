

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Default deconstructor for key value pairs
    /// </summary>
    [PublicAPI]
    public static class DeconstructorExtensions
    {
        /// <summary>
        /// Deconstructs the specified KVP.
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="kvp">The KVP.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void Deconstruct<TK, TV>(this KeyValuePair<TK, TV> kvp, out TK key, out TV value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
