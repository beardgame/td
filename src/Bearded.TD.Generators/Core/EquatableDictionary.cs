using System;
using System.Collections;
using System.Collections.Generic;

namespace Bearded.TD.Generators;

static class EquatableDictionary
{
    public static EquatableDictionary<TKey, TValue> AsEquatable<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary)
        where TKey : IEquatable<TKey> where TValue : IEquatable<TValue>
        => new(dictionary);
}

readonly struct EquatableDictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> implementation)
    : IReadOnlyDictionary<TKey, TValue>, IEquatable<EquatableDictionary<TKey, TValue>>
    where TKey : IEquatable<TKey>
    where TValue : IEquatable<TValue>
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => implementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)implementation).GetEnumerator();

    public int Count => implementation.Count;

    public bool ContainsKey(TKey key) => implementation.ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value) => implementation.TryGetValue(key, out value);

    public TValue this[TKey key] => implementation[key];

    public IEnumerable<TKey> Keys => implementation.Keys;

    public IEnumerable<TValue> Values => implementation.Values;

    public override bool Equals(object? obj) => obj is EquatableDictionary<TKey, TValue> other && Equals(other);

    public bool Equals(EquatableDictionary<TKey, TValue> other)
    {
        if (Count != other.Count)
            return false;

        foreach (var kvp in this)
        {
            if (!other.TryGetValue(kvp.Key, out var value) ||
                !kvp.Value.Equals(value))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        HashCode hashCode = default;

        foreach (var kvp in this)
        {
            hashCode.Add(kvp.Key);
            hashCode.Add(kvp.Value);
        }

        return hashCode.ToHashCode();
    }
}
