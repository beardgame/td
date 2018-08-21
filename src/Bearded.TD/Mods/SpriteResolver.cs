using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    sealed class SpriteResolver : IDependencyResolver<ISprite>
    {
        private readonly ModMetadata thisMod;
        private readonly ReadonlyBlueprintCollection<SpriteSet> thisModsSpriteSets;
        private readonly Dictionary<string, Mod> mods;

        public SpriteResolver(ModMetadata thisMod, ReadonlyBlueprintCollection<SpriteSet> thisModsSpriteSets, IEnumerable<Mod> otherMods)
        {
            this.thisMod = thisMod;
            this.thisModsSpriteSets = thisModsSpriteSets;
            mods = otherMods.ToDictionary(m => m.Id);
        }

        public ISprite Resolve(string id)
            => Resolve(ModAwareSpriteId.FromNameInMod(id, thisMod));

        public ISprite Resolve(ModAwareSpriteId id)
        {
            var modId = id.SpriteSet.ModId;

            if (modId == thisMod.Id)
            {
                return spriteFrom(thisModsSpriteSets, id);
            }

            if (mods.TryGetValue(modId, out var mod))
            {
                return spriteFrom(mod.Blueprints.Sprites, id);
            }

            throw new InvalidDataException($"Unknown mod in identifier {id}");
        }

        private ISprite spriteFrom(ReadonlyBlueprintCollection<SpriteSet> sets, ModAwareSpriteId id)
        {
            var spriteSetId = id.SpriteSet.Id;
            var spriteId = id.Id;

            var set = sets[spriteSetId];

            return set.Sprites.GetSprite(spriteId);
        }
    }
}
