using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Mods
{
    class ReadonlyBlueprintCollection<T> where T : IIdable<T>
    {
        private readonly ReadonlyIdDictionary<T> blueprintsById;

        public ReadonlyBlueprintCollection(IEnumerable<T> blueprints)
        {
            blueprintsById = new ReadonlyIdDictionary<T>(blueprints);
        }

        public ReadonlyBlueprintCollection(IDictionary<Id<T>, T> blueprints)
        {
            blueprintsById = new ReadonlyIdDictionary<T>(blueprints);
        }

        public T this[Id<T> id] => blueprintsById[id];
    }

    class ReadonlyNamedBlueprintCollection<T> : ReadonlyBlueprintCollection<T>  where T : IIdable<T>, INamed
    {
        private readonly IReadOnlyDictionary<string, T> blueprintsByName;

        public ReadonlyNamedBlueprintCollection(IEnumerable<T> blueprints)
            : base(blueprints)
        {
            blueprintsByName = blueprints.ToDictionary(blueprint => blueprint.Name);
        }

        public ReadonlyNamedBlueprintCollection(IDictionary<Id<T>, T> blueprints)
            : base(blueprints)
        {
            blueprintsByName = blueprints.Values.ToDictionary(blueprint => blueprint.Name);
        }
    }
}
