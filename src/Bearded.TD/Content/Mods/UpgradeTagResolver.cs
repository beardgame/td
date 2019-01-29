using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Content.Mods
{
    sealed class UpgradeTagResolver
    {
        private readonly ModMetadata thisMod;
        private readonly IDictionary<string, UpgradeTag> tagsForThisMod = new Dictionary<string, UpgradeTag>();
        private readonly IDictionary<string, Mod> mods;

        public UpgradeTagResolver(ModMetadata thisMod, IEnumerable<Mod> otherMods)
        {
            this.thisMod = thisMod;
            mods = otherMods.ToDictionary(m => m.Id);
        }

        public UpgradeTag Resolve(string id)
            => Resolve(ModAwareId.FromNameInMod(id, thisMod));

        public UpgradeTag Resolve(ModAwareId id)
        {
            if (id.ModId == thisMod.Id)
            {
                if (tagsForThisMod.TryGetValue(id.Id, out var existingTag))
                {
                    return existingTag;
                }
                var newTag = new UpgradeTag();
                tagsForThisMod.Add(id.Id, newTag);

                return newTag;
            }

            if (mods.TryGetValue(id.ModId, out var mod))
            {
                return mod.Tags.TryGetValue(id.Id, out var existingTag)
                    ? existingTag
                    : throw new InvalidDataException($"Unknown tag {id}");
            }
            
            throw new InvalidDataException($"Unknown mod in identifier {id}");
        }

        public IDictionary<string, UpgradeTag> GetForCurrentMod()
            => new ReadOnlyDictionary<string, UpgradeTag>(tagsForThisMod);
    }
}
