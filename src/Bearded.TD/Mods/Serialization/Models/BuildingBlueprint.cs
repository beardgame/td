using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class BuildingBlueprint
        : IConvertsTo<Mods.Models.BuildingBlueprint, DependencyResolver<Mods.Models.FootprintGroup>>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Footprint { get; set; }
        public int Health { get; set; }
        public int Cost { get; set; }
        public List<IBuildingComponent> Components { get; set; }

        // how to component?

        public Mods.Models.BuildingBlueprint ToGameModel(
            DependencyResolver<Mods.Models.FootprintGroup> footprints)
        {
            return new Mods.Models.BuildingBlueprint(
                Id,
                Name,
                footprints.Resolve(Footprint),
                Health,
                Cost,
                makeComponentFactories()
            );
        }

        private IEnumerable<IBuildingComponentFactory> makeComponentFactories()
        {
            return Components.Select(
                c => ComponentFactories.Buildings[c.Id](c)
            );
        }
    }
}
