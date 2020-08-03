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

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue value)
        {
            if (d.TryGetValue(key, out TValue value2))
                return value2;

            d.Add(key, value);

            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key)
            where TValue : new()
            => d.GetOrAdd(key, new TValue());
    }
}
