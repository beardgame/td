using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bearded.TD.Game
{
    interface IBlueprintCollection<out T> where T : IBlueprint
    {
        T this[string id] { get; }
        IEnumerable<T> All { get; }
    }

    sealed class BlueprintCollection<T> : IBlueprintCollection<T> where T : IBlueprint
    {
        private readonly Dictionary<string, T> blueprintsById = new Dictionary<string, T>();

        public T this[string id] => blueprintsById[id];
        public IEnumerable<T> All => blueprintsById.Values;

        public void Add(T blueprint)
        {
            blueprintsById.Add(blueprint.Id, blueprint);
        }

        public ReadonlyBlueprintCollection<T> AsReadonly() => new ReadonlyBlueprintCollection<T>(blueprintsById);
    }

    sealed class ReadonlyBlueprintCollection<T> : IBlueprintCollection<T> where T : IBlueprint
    {
        private readonly IReadOnlyDictionary<string, T> blueprintsById;

        public ReadonlyBlueprintCollection(IEnumerable<T> blueprints)
            : this(blueprints.ToDictionary(blueprint => blueprint.Id)) { }

        public ReadonlyBlueprintCollection(IDictionary<string, T> blueprints)
        {
            blueprintsById = new ReadOnlyDictionary<string, T>(blueprints);
        }

        public T this[string id] => blueprintsById[id];
        public IEnumerable<T> All => blueprintsById.Values;

        public static implicit operator ReadonlyBlueprintCollection<T>(ReadonlyBlueprintCollection.EmptyReadonlyBlueprintCollection empty)
            => new ReadonlyBlueprintCollection<T>(Enumerable.Empty<T>());
    }

    static class ReadonlyBlueprintCollection
    {
        public struct EmptyReadonlyBlueprintCollection { }

        public static EmptyReadonlyBlueprintCollection Empty => new EmptyReadonlyBlueprintCollection();
    }
}
