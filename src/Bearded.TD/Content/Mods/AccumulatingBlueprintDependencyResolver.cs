using System;
using System.Collections.Generic;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Mods
{
    sealed class AccumulatingBlueprintDependencyResolver<T> : ModAwareDependencyResolver<T>
        where T : IBlueprint
    {
        private readonly BlueprintCollection<T> thisModsBlueprints;
        private readonly Func<Mod, ReadonlyBlueprintCollection<T>> blueprintsOf;

        public AccumulatingBlueprintDependencyResolver(
            ModMetadata thisMod,
            BlueprintCollection<T> thisModsBlueprints,
            IEnumerable<Mod> otherMods,
            Func<Mod, ReadonlyBlueprintCollection<T>> blueprintCollectionSelector)
            : base(thisMod, otherMods)
        {
            this.thisModsBlueprints = thisModsBlueprints;
            blueprintsOf = blueprintCollectionSelector;
        }

        protected override T getDependencyFromThisMod(string id) => thisModsBlueprints[id];

        protected override T getDependencyFromOtherMod(Mod mod, string id) => blueprintsOf(mod)[id];
    }
}
