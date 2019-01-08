using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Utilities.Collections
{
    class IdCollection<T> where T : IIdable<T>
    {
        private readonly List<T> objects = new List<T>();
        private readonly IdDictionary<T> objectsById = new IdDictionary<T>();
        public ReadOnlyCollection<T> AsReadOnly { get; }
        public ImmutableDictionary<Id<T>, T> AsImmutableDictionary
            => objectsById.ToImmutableDictionary(entry => entry.Key, entry => entry.Value);

        public IdCollection()
        {
            AsReadOnly = objects.AsReadOnly();
        }

        public void Add(T obj)
        {
            objects.Add(obj);
            objectsById.Add(obj);
        }

        public bool Remove(T obj)
        {
            objectsById.Remove(obj);
            return objects.Remove(obj);
        }

        public T this[Id<T> id] => objectsById[id];
    }
}
