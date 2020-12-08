using System;
using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Game.GameState;

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

        protected override T GetDependencyFromThisMod(ModAwareId id) => thisModsBlueprints[id];

        protected override T GetDependencyFromOtherMod(Mod mod, ModAwareId id) => blueprintsOf(mod)[id];
    }
}
