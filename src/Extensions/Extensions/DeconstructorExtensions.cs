

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public static class DeconstructorExtensions
    {
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> kvp, out K key, out V value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
