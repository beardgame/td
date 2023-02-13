using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Bearded.TD.Utilities.Collections;

sealed class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, List<TValue>>>
    where TKey : notnull
{
    private readonly Dictionary<TKey, List<TValue>> inner = new();

    public bool ContainsKey(TKey key) => inner.TryGetValue(key, out var list) && list.Count > 0;

    public void Add(TKey key, TValue value)
    {
        if (!inner.TryGetValue(key, out var list))
        {
            list = new List<TValue>();
            inner.Add(key, list);
        }
        list.Add(value);
    }

    public bool RemoveAll(TKey key)
    {
        return inner.Remove(key);
    }

    public bool Remove(TKey key, TValue value)
    {
        return inner.TryGetValue(key, out var list) && list.Remove(value);
    }

    public int RemoveWhere(TKey key, Predicate<TValue> match)
    {
        if (!inner.TryGetValue(key, out var list) || list.Count == 0)
            return 0;

        return list.RemoveAll(match);
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out IEnumerable<TValue>? values)
    {
        var result = inner.TryGetValue(key, out var list);
        values = list;
        return result;
    }

    public IEnumerable<TValue> Get(TKey key)
    {
        return inner.TryGetValue(key, out var list) ? list : Enumerable.Empty<TValue>();
    }

    public void Clear()
    {
        inner.Clear();
    }

    public IEnumerable<TValue> this[TKey key] => Get(key);

    public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator() => inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
