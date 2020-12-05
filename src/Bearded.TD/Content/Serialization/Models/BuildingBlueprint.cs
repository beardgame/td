using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Resources;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    sealed class BuildingBlueprint
        : IConvertsTo<Content.Models.BuildingBlueprint, UpgradeTagResolver>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Game.World.FootprintGroup Footprint { get; set; }
        public ResourceAmount Cost { get; set; }
        public List<string> Tags { get; set; }
        public List<IBuildingComponent> Components { get; set; }

        public Content.Models.BuildingBlueprint ToGameModel(ModMetadata modMetadata, UpgradeTagResolver tags)
        {
            return new Content.Models.BuildingBlueprint(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Footprint,
                Cost,
                Tags?.Select(tags.Resolve),
                Components?.Select(ComponentFactories.CreateBuildingComponentFactory)
            );
        }
    }
}
