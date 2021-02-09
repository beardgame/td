using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Mods
{
    sealed class SpriteResolver : IDependencyResolver<ISpriteBlueprint>
    {
        private readonly ModMetadata thisMod;
        private readonly ReadonlyBlueprintCollection<SpriteSet> thisModsSpriteSets;
        private readonly Dictionary<string, Mod> mods;

        public SpriteResolver(ModMetadata thisMod, ReadonlyBlueprintCollection<SpriteSet> thisModsSpriteSets,
            IEnumerable<Mod> otherMods)
        {
            this.thisMod = thisMod;
            this.thisModsSpriteSets = thisModsSpriteSets;
            mods = otherMods.ToDictionary(m => m.Id);
        }

        public ISpriteBlueprint Resolve(string id)
            => Resolve(ModAwareSpriteId.FromNameInMod(id, thisMod));

        public ISpriteBlueprint Resolve(ModAwareSpriteId id)
        {
            // TODO: consider extracting shared branching from ModAwareDependencyResolver
            // (but note that it is using a different id type)
            var modId = id.SpriteSet.ModId;

            try
            {
                if (modId == thisMod.Id)
                {
                    return spriteFrom(thisModsSpriteSets, id);
                }

                if (mods.TryGetValue(modId, out var mod))
                {
                    return spriteFrom(mod.Blueprints.Sprites, id);
                }
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Failed to find sprite with id \"{id}\".", e);
            }

            throw new InvalidDataException($"Unknown mod in identifier {id}");
        }

        private ISpriteBlueprint spriteFrom(ReadonlyBlueprintCollection<SpriteSet> sets, ModAwareSpriteId id)
        {
            var spriteId = id.Id;

            var set = sets[id.SpriteSet];

            return set.GetSprite(spriteId);
        }
    }
}
