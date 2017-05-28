using System.Collections.Generic;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Blueprints
{
    class BlueprintCollection<T> where T : IIdable<T>
    {
        private readonly Dictionary<T> blueprintsById = new Dictionary<T>();
        private readonly Dictionary<string, T> blueprintsByName = new Dictionary<string, T>();

        public T this[Id<T> id] => blueprintsById[id];
        public T this[string name] => blueprintsByName[name];

        public void RegisterBlueprint(string name, T blueprint)
        {
            blueprintsById.Add(blueprint);
            blueprintsByName.Add(name, blueprint);
        }
    }
}