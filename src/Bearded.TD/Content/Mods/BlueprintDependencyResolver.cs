using System;
using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Utilities;

namespace Bearded.TD.Content.Mods
{
    sealed class BlueprintDependencyResolver<T> : ModAwareDependencyResolver<T>
        where T : IBlueprint
    {
        private readonly ReadonlyBlueprintCollection<T> thisModsBlueprints;
        private readonly Func<Mod, ReadonlyBlueprintCollection<T>> blueprintsOf;

        public BlueprintDependencyResolver(ModMetadata thisMod, ReadonlyBlueprintCollection<T> thisModsBlueprints,
            IEnumerable<Mod> otherMods, Func<Mod, ReadonlyBlueprintCollection<T>> blueprintCollectionSelector)
            : base(thisMod, otherMods)
        {
            this.thisModsBlueprints = thisModsBlueprints;
            blueprintsOf = blueprintCollectionSelector;
        }

        protected override T GetDependencyFromThisMod(ModAwareId id)
        {
            return thisModsBlueprints[id];
        }

        protected override T GetDependencyFromOtherMod(Mod mod, ModAwareId id)
        {
            DebugAssert.Argument.Satisfies(mod.Id == id.ModId);
            return blueprintsOf(mod)[id];
        }
    }
}
