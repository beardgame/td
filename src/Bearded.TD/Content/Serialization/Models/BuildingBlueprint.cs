using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class BuildingBlueprint
        : IConvertsTo<Content.Models.BuildingBlueprint, UpgradeTagResolver>
    {
        public string? Id { get; set; }
        public List<IBuildingComponent>? Components { get; set; }

        public Content.Models.BuildingBlueprint ToGameModel(ModMetadata modMetadata, UpgradeTagResolver tags)
        {
            _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");

            return new Content.Models.BuildingBlueprint(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Components?.Select(ComponentFactories.CreateComponentFactory<Building>)
                    ?? Enumerable.Empty<IComponentFactory<Building>>()
            );
        }
    }
}
