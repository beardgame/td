using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Upgrades;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class BuildingBlueprint
        : IConvertsTo<Content.Models.BuildingBlueprint, UpgradeTagResolver>
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<string>? Tags { get; set; }
        public List<IBuildingComponent>? Components { get; set; }

        public Content.Models.BuildingBlueprint ToGameModel(ModMetadata modMetadata, UpgradeTagResolver tags)
        {
            _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
            _ = Name ?? throw new InvalidDataException($"{nameof(Name)} must be non-null");

            return new(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Tags?.Select(tags.Resolve)
                    ?? Enumerable.Empty<UpgradeTag>(),
                Components?.Select(ComponentFactories.CreateBuildingComponentFactory)
                    ?? Enumerable.Empty<BuildingComponentFactory>()
            );
        }
    }
}
