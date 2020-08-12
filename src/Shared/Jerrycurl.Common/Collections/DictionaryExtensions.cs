using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Jerrycurl.Collections
{
    internal static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue defaultValue = default)
        {
            if (d == null)
                throw new ArgumentNullException(nameof(d));

            if (d.TryGetValue(key, out TValue value))
                return value;

            return defaultValue;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue value) => d.GetOrAdd(key, () => value);
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, Func<TValue> valueFactory)
        {
            if (d.TryGetValue(key, out TValue value2))
                return value2;

            TValue value;

            d.Add(key, value = valueFactory());

            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key)
            where TValue : new()
            => d.GetOrAdd(key, new TValue());

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue value)
        {
            if (d.ContainsKey(key))
                return false;

            d.Add(key, value);

            return true;
        }

        public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key)
        {
            if (!d.ContainsKey(key))
                return false;

            d.Remove(key);

            return true;
        }
    }
}
