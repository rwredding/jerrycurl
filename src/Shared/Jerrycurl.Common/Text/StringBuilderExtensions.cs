using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Text
{
    internal static class StringBuilderExtensions
    {
#if NETSTANDARD2_0
        public static StringBuilder AppendJoin<T>(this StringBuilder builder, string separator, IEnumerable<T> values)
            => builder.Append(string.Join(separator, values));
#endif
    }
}
