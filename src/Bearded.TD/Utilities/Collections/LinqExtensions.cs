using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities;

namespace Bearded.TD.Utilities.Collections
{
    static class LinqExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
            where TKey : notnull
        {
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static TValue GetOrInsert<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
            TKey key, Func<TValue> getValueToInsert)
            where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            value = getValueToInsert();
            dictionary.Add(key, value);
            return value;
        }

        public static TValue GetValueOrInsertNewDefaultFor<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            value = new TValue();
            dictionary.Add(key, value);
            return value;
        }

        public static Maybe<TValue> MaybeValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
        {
            return dictionary.TryGetValue(key, out var value) ? Maybe.Just(value) : Maybe.Nothing;
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static TValue ValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
            where TKey : notnull
        {
            dict.TryGetValue(key, out var value);
            return value;
        }

        public static IEnumerable<(T, int)> Indexed<T>(this IEnumerable<T> source)
            => source.Select((t, i) => (t, i));

        public static IEnumerable<TResult> ConsecutivePairs<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TSource, TResult> selector)
        {
            using var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
                yield break;

            var previous = enumerator.Current;

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                yield return selector(previous, current);

                previous = current;
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source == null || !source.Any();

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Predicate<T> predicate)
            => source.Where(item => !predicate(item));
    }
}
