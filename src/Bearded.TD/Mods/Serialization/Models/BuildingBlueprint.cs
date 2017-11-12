
using System.Linq;
using Bearded.TD.Game.Buildings;

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class BuildingBlueprint
    {
        public string Name { get; set; }
        public string Footprint { get; set; }
        public int Health { get; set; }
        public int Cost { get; set; }

        // how to component?

        public Mods.Models.BuildingBlueprint ToGameModel(DependencyResolver<Tiles.FootprintGroup> footprints)
        {
            return new Mods.Models.BuildingBlueprint(
                Name,
                footprints.Resolve(Footprint),
                Health,
                Cost,
                Enumerable.Empty<ComponentFactory>()
                );
        }
    }
}
