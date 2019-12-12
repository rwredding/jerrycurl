using System;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Collections
{
    internal static class EnumerableExtensions
    {
        public static T Second<T>(this IEnumerable<T> source) => source.Skip(1).First();
        public static T SecondOrDefault<T>(this IEnumerable<T> source) => source.Skip(1).FirstOrDefault();

        public static HashSet<T> ToSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);
        public static HashSet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) => new HashSet<T>(source, comparer);

#if !NETCOREAPP3_0
        public static IEnumerable<(TFirst l, TSecond r)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> source, IEnumerable<TSecond> second) => source.Zip(second, (l, r) => (l, r));
#endif
        public static IEnumerable<(TFirst l, TSecond r)> ZipOuter<TFirst, TSecond>(this IEnumerable<TFirst> source, IEnumerable<TSecond> second)
        {
#pragma warning disable IDE0063
            using (IEnumerator<TFirst> e1 = source.GetEnumerator())
            {
                using (IEnumerator<TSecond> e2 = second.GetEnumerator())
                {
                    bool b1, b2;

                    while ((b1 = e1.MoveNext()) | (b2 = e2.MoveNext()))
                    {
                        yield return (
                            b1 ? e1.Current : default,
                            b2 ? e2.Current : default
                        );
                    }
                }
            }
        }
#pragma warning restore IDE0063

        public static IEnumerable<TResult> NotNull<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
            where TResult : class
            => source.Select(selector).NotNull();

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
            where T : class
            => source.Where(it => it != null);

        public static IEnumerable<T> AsLazy<T>(this IEnumerable<T> source)
        {
            Lazy<IReadOnlyList<T>> lazy = new Lazy<IReadOnlyList<T>>(() => source.ToList(), false);

            foreach (T item in lazy.Value)
                yield return item;
        }
    }
}
