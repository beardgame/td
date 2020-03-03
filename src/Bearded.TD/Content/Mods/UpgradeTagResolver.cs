using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Content.Mods
{
    sealed class UpgradeTagResolver : ModAwareDependencyResolver<UpgradeTag>
    {
        private readonly IDictionary<string, UpgradeTag> tagsForThisMod = new Dictionary<string, UpgradeTag>();

        public UpgradeTagResolver(ModMetadata thisMod, IEnumerable<Mod> otherMods)
            : base(thisMod, otherMods)
        {
        }

        protected override UpgradeTag getDependencyFromThisMod(string id)
        {
            if (tagsForThisMod.TryGetValue(id, out var existingTag))
            {
                return existingTag;
            }

            var newTag = new UpgradeTag();
            tagsForThisMod.Add(id, newTag);

            return newTag;
        }

        protected override UpgradeTag getDependencyFromOtherMod(Mod mod, string id)
        {
            return mod.Tags.TryGetValue(id, out var existingTag)
                ? existingTag
                : throw new InvalidDataException($"Unknown tag {id}");
        }

        public IDictionary<string, UpgradeTag> GetForCurrentMod()
            => new ReadOnlyDictionary<string, UpgradeTag>(tagsForThisMod);
    }
}
