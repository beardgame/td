using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities;

namespace Bearded.TD.Utilities.Collections
{
    static class LinqExtensions
    {
        public static TValue GetOrInsert<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
            TKey key, Func<TValue> getValueToInsert)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            value = getValueToInsert();
            dictionary.Add(key, value);
            return value;
        }

        public static TValue GetValueOrInsertNewDefaultFor<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            value = new TValue();
            dictionary.Add(key, value);
            return value;
        }

        public static Maybe<TValue> MaybeValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? Maybe.Just(value) : Maybe.Nothing;
        }
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

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => source == null || !source.Any();

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Predicate<T> predicate)
            => source.Where(item => !predicate(item));
    }
}
