using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.Interaction
{
    class BuildingClickHandler : IClickHandler
    {
        private readonly BuildingBlueprint blueprint;
        private BuildingGhost ghost;

        public TileSelection Selection => blueprint.FootprintSelector;

        public BuildingClickHandler(BuildingBlueprint blueprint)
        {
            this.blueprint = blueprint;
        }

        public void HandleHover(GameState game, PositionedFootprint footprint)
        {
            ghost.SetFootprint(footprint);
        }

        public void HandleClick(GameState game, PositionedFootprint footprint)
        {
            if (footprint.OccupiedTiles.Any((tile) => !tile.IsValid || !tile.Info.IsPassable)) return;

            game.Add(new PlayerBuilding(blueprint, footprint));
        }

        public void Enable(GameState game)
        {
            game.Add(ghost = new BuildingGhost(blueprint));
        }

        public void Disable(GameState game)
        {
            ghost.Delete();
        }
    }
}
