using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class BuildingBlueprint
        : IConvertsTo<Content.Models.BuildingBlueprint,
                (DependencyResolver<Game.World.FootprintGroup> footprints, UpgradeTagResolver tags)>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Footprint { get; set; }
        public int Cost { get; set; }
        public List<string> Tags { get; set; }
        public List<IBuildingComponent> Components { get; set; }
        
        public Content.Models.BuildingBlueprint ToGameModel(
            (DependencyResolver<Game.World.FootprintGroup> footprints, UpgradeTagResolver tags) dependencies)
        {
            return new Content.Models.BuildingBlueprint(
                Id,
                Name,
                dependencies.footprints.Resolve(Footprint),
                Cost,
                Tags?.Select(dependencies.tags.Resolve),
                Components?.Select(ComponentFactories.CreateBuildingComponentFactory)
            );
        }
    }
}
