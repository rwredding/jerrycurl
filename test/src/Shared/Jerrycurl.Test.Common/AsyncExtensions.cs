using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Test
{
#if NETCOREAPP3_0
    public static class AsyncExtensions
    {
        public static async Task<TItem> FirstOrDefault<TItem>(this IAsyncEnumerable<TItem> enumerable)
        {
            await foreach (TItem item in enumerable)
                return item;

            return default;
        }

        public static async Task<IList<TItem>> ToList<TItem>(this IAsyncEnumerable<TItem> enumerable)
        {
            List<TItem> items = new List<TItem>();

            await foreach (TItem item in enumerable)
                items.Add(item);

            return items;
        }
    }
#endif
}
