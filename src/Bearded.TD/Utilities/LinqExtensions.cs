using System;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Utilities
{
    static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static TValue ValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            dict.TryGetValue(key, out var value);
            return value;
        }

        public static IEnumerable<(T, int)> Indexed<T>(this IEnumerable<T> source)
            => source.Select((t, i) => (t, i));

        public static IEnumerable<(T one, T next)> ConsecutivePairs<T>(this IEnumerable<T> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    yield break;

                var one = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    var other = enumerator.Current;

                    yield return (one, other);

                    one = other;
                }
            }
        }
    }
}