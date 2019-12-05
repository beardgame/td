using System.Collections;
using System.Collections.Generic;

namespace Bearded.TD.Utilities.Collections
{
    sealed class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, IList<TValue>>>
    {
        private static readonly TValue[] empty = {};

        private readonly Dictionary<TKey, IList<TValue>> inner = new Dictionary<TKey, IList<TValue>>();

        public bool ContainsKey(TKey key) => inner.ContainsKey(key);

        public void Add(TKey key, TValue value)
        {
            if (!ContainsKey(key))
            {
                inner.Add(key, new List<TValue>());
            }
            inner[key].Add(value);
        }

        public bool RemoveAll(TKey key)
        {
            return inner.Remove(key);
        }

        public bool Remove(TKey key, TValue value)
        {
            return ContainsKey(key) && inner[key].Remove(value);
        }

        public IEnumerable<TValue> Get(TKey key)
        {
            return ContainsKey(key) ? inner[key] : empty;
        }

        public IEnumerable<TValue> this[TKey key] => Get(key);

        public IEnumerator<KeyValuePair<TKey, IList<TValue>>> GetEnumerator() => inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
