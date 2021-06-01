using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class UpgradeBlueprint
        : IConvertsTo<Content.Models.UpgradeBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ResourceAmount Cost { get; set; }
        public List<IUpgradeEffect> Effects { get; set; }

        public Content.Models.UpgradeBlueprint ToGameModel(ModMetadata modMetadata, Void resolvers)
        {
            return new(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Cost,
                ImmutableArray.CreateRange(Effects));
        }
    }
}
