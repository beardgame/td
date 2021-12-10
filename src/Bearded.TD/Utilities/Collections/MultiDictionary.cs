using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Utilities.Collections;

sealed class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, IList<TValue>>>
    where TKey : notnull
{
    private static readonly TValue[] empty = Array.Empty<TValue>();

    private readonly Dictionary<TKey, IList<TValue>> inner = new();

    public bool ContainsKey(TKey key) => inner.ContainsKey(key);

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
        return ContainsKey(key) && inner[key].Remove(value);
    }

    public int RemoveWhere(TKey key, Predicate<TValue> match)
    {
        if (!inner.TryGetValue(key, out var list) || list.Count == 0)
            return 0;

        if (list is List<TValue> listImpl)
        {
            return listImpl.RemoveAll(match);
        }

        var toRemove = list.Where(x => match(x)).ToList();
        foreach (var value in toRemove)
        {
            list.Remove(value);
        }
        return toRemove.Count;
    }

    public bool TryGetValue(TKey key, out IEnumerable<TValue> list)
    {
        list = Get(key);
        return ContainsKey(key);
    }

    public IEnumerable<TValue> Get(TKey key)
    {
        return ContainsKey(key) ? inner[key] : empty;
    }

    public IEnumerable<TValue> this[TKey key] => Get(key);

    public IEnumerator<KeyValuePair<TKey, IList<TValue>>> GetEnumerator() => inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}