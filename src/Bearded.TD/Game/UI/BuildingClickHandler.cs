using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
{
    class BuildingClickHandler : IClickHandler
    {
        private readonly Faction faction;
        private readonly BuildingBlueprint blueprint;
        private BuildingGhost ghost;

        public TileSelection Selection => TileSelection.FromFootprints(blueprint.Footprints);

        public BuildingClickHandler(Faction faction, BuildingBlueprint blueprint)
        {
            this.faction = faction;
            this.blueprint = blueprint;
        }

        public void HandleHover(GameInstance game, PositionedFootprint footprint)
        {
            ghost.SetFootprint(footprint);
        }

        public void HandleClick(GameInstance game, PositionedFootprint footprint)
        {
            game.Request(BuildBuilding.Request, faction, blueprint, footprint);
        }

        public void Enable(GameInstance game)
        {
            ghost = new BuildingGhost(blueprint);
            game.State.Add(ghost);
        }

        public void Disable(GameInstance game)
        {
            ghost.Delete();
        }
    }
}
