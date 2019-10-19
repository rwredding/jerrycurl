using System;
using System.Collections.Generic;

namespace Jerrycurl.Collections
{
    internal static class DictionaryExtensions
    {
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key)
        {
            if (d == null)
                throw new ArgumentNullException(nameof(d));

            if (d.TryGetValue(key, out TValue value))
                return value;

            return default;
        }
    }
}
