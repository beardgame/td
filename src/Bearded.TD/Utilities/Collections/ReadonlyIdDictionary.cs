using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Utilities.Collections
{
    sealed class ReadonlyIdDictionary<T> : IReadOnlyDictionary<Id<T>, T> where T : IIdable<T>
    {
        private readonly IReadOnlyDictionary<Id<T>, T> dictionary;

        public ReadonlyIdDictionary(IDictionary<Id<T>, T> original)
        {
            dictionary = new ReadOnlyDictionary<Id<T>, T>(original);
        }

        public ReadonlyIdDictionary(IEnumerable<T> values)
        {
            dictionary = values.ToDictionary(val => val.Id, val => val);
        }

        public IEnumerator<KeyValuePair<Id<T>, T>> GetEnumerator() => dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => dictionary.Count;

        public bool ContainsKey(Id<T> key) => dictionary.ContainsKey(key);

        public bool TryGetValue(Id<T> key, out T value) => dictionary.TryGetValue(key, out value);

        public T this[Id<T> key] => dictionary[key];

        public IEnumerable<Id<T>> Keys => dictionary.Keys;
        public IEnumerable<T> Values => dictionary.Values;
    }
}
