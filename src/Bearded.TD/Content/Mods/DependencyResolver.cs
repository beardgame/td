using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Mods
{
    sealed class DependencyResolver<T> : IDependencyResolver<T>
        where T : IBlueprint
    {
        private readonly ModMetadata thisMod;
        private readonly Dictionary<string, Mod> mods;
        private readonly ReadonlyBlueprintCollection<T> thisModsBlueprints;
        private readonly Func<Mod, ReadonlyBlueprintCollection<T>> blueprintsOf;

        public DependencyResolver(ModMetadata thisMod, ReadonlyBlueprintCollection<T> thisModsBlueprints,
            IEnumerable<Mod> otherMods, Func<Mod, ReadonlyBlueprintCollection<T>> blueprintCollectionSelector)
        {
            this.thisMod = thisMod;
            this.thisModsBlueprints = thisModsBlueprints;
            blueprintsOf = blueprintCollectionSelector;
            mods = otherMods.ToDictionary(m => m.Id);
        }

        public T Resolve(string id)
            => Resolve(ModAwareId.FromNameInMod(id, thisMod));

        public T Resolve(ModAwareId id)
        {
            if (id.ModId == thisMod.Id)
            {
                return thisModsBlueprints[id.Id];
            }

            if (mods.TryGetValue(id.ModId, out var mod))
            {
                return blueprintsOf(mod)[id.Id];
            }
            
            throw new InvalidDataException($"Unknown mod in identifier {id}");
        }
    }
}
