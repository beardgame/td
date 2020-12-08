using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Bearded.TD.Game.GameState.Upgrades;

namespace Bearded.TD.Content.Mods
{
    sealed class UpgradeTagResolver : ModAwareDependencyResolver<UpgradeTag>
    {
        private readonly IDictionary<ModAwareId, UpgradeTag> tagsForThisMod = new Dictionary<ModAwareId, UpgradeTag>();

        public UpgradeTagResolver(ModMetadata thisMod, IEnumerable<Mod> otherMods)
            : base(thisMod, otherMods)
        {
        }

        protected override UpgradeTag GetDependencyFromThisMod(ModAwareId id)
        {
            if (tagsForThisMod.TryGetValue(id, out var existingTag))
            {
                return existingTag;
            }

            var newTag = new UpgradeTag();
            tagsForThisMod.Add(id, newTag);

            return newTag;
        }

        protected override UpgradeTag GetDependencyFromOtherMod(Mod mod, ModAwareId id)
        {
            return mod.Tags.TryGetValue(id, out var existingTag)
                ? existingTag
                : throw new InvalidDataException($"Unknown tag {id}");
        }

        public IDictionary<ModAwareId, UpgradeTag> GetForCurrentMod()
            => new ReadOnlyDictionary<ModAwareId, UpgradeTag>(tagsForThisMod);
    }
}
