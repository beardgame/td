using System.Collections.Generic;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Blueprints
{
    class BlueprintCollection<T> where T : IIdable<T>
    {
        private readonly IdDictionary<T> blueprintsById = new IdDictionary<T>();

        public T this[Id<T> id] => blueprintsById[id];

        public virtual void RegisterBlueprint(T blueprint)
        {
            blueprintsById.Add(blueprint);
        }

        public IEnumerable<T> All => blueprintsById.Values;
    }

    class NamedBlueprintCollection<T> : BlueprintCollection<T> where T : IIdable<T>, INamed
    {
        private readonly Dictionary<string, T> blueprintsByName = new Dictionary<string, T>();

        public T this[string name] => blueprintsByName[name];

        public override void RegisterBlueprint(T blueprint)
        {
            base.RegisterBlueprint(blueprint);
            blueprintsByName.Add(blueprint.Name, blueprint);
        }
    }
}
