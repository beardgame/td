using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Upgrades;
using Bearded.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    sealed class UpgradeBlueprint
        : IConvertsTo<Content.Models.UpgradeBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public List<IUpgradeEffect> Effects { get; set; }

        public Content.Models.UpgradeBlueprint ToGameModel(ModMetadata modMetadata, Void resolvers)
        {
            return new Content.Models.UpgradeBlueprint(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Cost,
                ImmutableArray.CreateRange(Effects));
        }
    }
}
