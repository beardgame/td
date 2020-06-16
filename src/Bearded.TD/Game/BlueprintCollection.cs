using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game
{
    interface IBlueprintCollection<out T> where T : IBlueprint
    {
        T this[ModAwareId id] { get; }
        IEnumerable<T> All { get; }
    }

    sealed class BlueprintCollection<T> : IBlueprintCollection<T> where T : IBlueprint
    {
        private readonly Dictionary<ModAwareId, T> blueprintsById = new Dictionary<ModAwareId, T>();

        public T this[ModAwareId id] => blueprintsById[id];
        public IEnumerable<T> All => blueprintsById.Values;

        public void Add(T blueprint)
        {
            blueprintsById.Add(blueprint.Id, blueprint);
        }

        public ReadonlyBlueprintCollection<T> AsReadonly() => new ReadonlyBlueprintCollection<T>(blueprintsById);
    }

    sealed class ReadonlyBlueprintCollection<T> : IBlueprintCollection<T> where T : IBlueprint
    {
        private readonly IReadOnlyDictionary<ModAwareId, T> blueprintsById;

        public ReadonlyBlueprintCollection(IEnumerable<T> blueprints)
            : this(blueprints.ToDictionary(blueprint => blueprint.Id)) { }

        public ReadonlyBlueprintCollection(IDictionary<ModAwareId, T> blueprints)
        {
            blueprintsById = new ReadOnlyDictionary<ModAwareId, T>(blueprints);
        }

        public T this[ModAwareId id] => blueprintsById[id];
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
