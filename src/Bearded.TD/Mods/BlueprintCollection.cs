using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    interface IBlueprintCollection<out T> where T : IBlueprint
    {
        T this[string name] { get; }
        IEnumerable<T> All { get; }
    }

    sealed class BlueprintCollection<T> : IBlueprintCollection<T> where T : IBlueprint
    {
        private readonly Dictionary<string, T> blueprintsByName = new Dictionary<string, T>();

        public T this[string name] => blueprintsByName[name];
        public IEnumerable<T> All => blueprintsByName.Values;

        public void Add(T blueprint)
        {
            blueprintsByName.Add(blueprint.Name, blueprint);
        }

        public ReadonlyBlueprintCollection<T> AsReadonly() => new ReadonlyBlueprintCollection<T>(blueprintsByName);
    }

    sealed class ReadonlyBlueprintCollection<T> : IBlueprintCollection<T> where T : IBlueprint
    {
        private readonly IReadOnlyDictionary<string, T> blueprintsByName;

        public ReadonlyBlueprintCollection(IEnumerable<T> blueprints)
            : this(blueprints.ToDictionary(blueprint => blueprint.Name)) { }

        public ReadonlyBlueprintCollection(IDictionary<string, T> blueprints)
        {
            blueprintsByName = new ReadOnlyDictionary<string, T>(blueprints);
        }

        public T this[string name] => blueprintsByName[name];
        public IEnumerable<T> All => blueprintsByName.Values;
    }
}
