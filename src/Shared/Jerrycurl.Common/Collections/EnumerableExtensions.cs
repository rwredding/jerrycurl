using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Collections
{
    internal static class EnumerableExtensions
    {
        public static T Second<T>(this IEnumerable<T> source) => source.Skip(1).First();
        public static T SecondOrDefault<T>(this IEnumerable<T> source) => source.Skip(1).FirstOrDefault();

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T element) => source.Except(new[] { element });

        public static HashSet<T> ToSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);
        public static HashSet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) => new HashSet<T>(source, comparer);

        public static T FirstOfType<T>(this IEnumerable source) => source.OfType<T>().FirstOrDefault();
        public static T FirstOfType<T>(this IEnumerable source, Func<T, bool> predicate) => source.OfType<T>().FirstOrDefault(predicate);

#if !NETCOREAPP3_0
        public static IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second) => first.Zip(second, (First, Second) => (First, Second));
#endif
        public static IEnumerable<(TFirst First, TSecond Second)> ZipAll<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
        {
#pragma warning disable IDE0063
            using (IEnumerator<TFirst> e1 = first.GetEnumerator())
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

#if !NETSTANDARD2_1 && !NETCOREAPP3_0
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T value) => source.Concat(new[] { value });
#endif
    }
}
