using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;
// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class BuildingBlueprint
        : IConvertsTo<Mods.Models.BuildingBlueprint,
                (DependencyResolver<Game.World.FootprintGroup> footprints, UpgradeTagResolver tags)>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Footprint { get; set; }
        public int Health { get; set; }
        public int Cost { get; set; }
        public List<string> Tags { get; set; }
        public List<IBuildingComponent> Components { get; set; }
        
        public Mods.Models.BuildingBlueprint ToGameModel(
            (DependencyResolver<Game.World.FootprintGroup> footprints, UpgradeTagResolver tags) dependencies)
        {
            return new Mods.Models.BuildingBlueprint(
                Id,
                Name,
                dependencies.footprints.Resolve(Footprint),
                Health,
                Cost,
                Tags?.Select(dependencies.tags.Resolve),
                Components?.Select(ComponentFactories.CreateBuildingComponentFactory)
            );
        }
    }
}
