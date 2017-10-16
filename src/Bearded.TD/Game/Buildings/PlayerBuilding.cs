using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.Utilities;

namespace Bearded.TD.Game.Buildings
{
    class PlayerBuilding : Building
    {
        public PlayerBuilding(Id<Building> id, BuildingBlueprint blueprint, PositionedFootprint footprint, Faction faction)
            : base(id, blueprint, footprint, faction)
        { }
    }
}
