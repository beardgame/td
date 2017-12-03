using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Buildings
{
    class BuildingPlaceholder : PlacedBuildingBase<BuildingPlaceholder>
    {
        public BuildingPlaceholder(BuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
        }

        protected override IEnumerable<IComponent<BuildingPlaceholder>> InitialiseComponents()
            => Enumerable.Empty<IComponent<BuildingPlaceholder>>();
    }
}