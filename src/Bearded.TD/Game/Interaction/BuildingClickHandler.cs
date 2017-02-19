using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Interaction
{
    class BuildingClickHandler : IClickHandler
    {
        private readonly BuildingBlueprint blueprint;
        private BuildingGhost ghost;

        public Footprint Footprint => blueprint.Footprint;

        public BuildingClickHandler(BuildingBlueprint blueprint)
        {
            this.blueprint = blueprint;
        }

        public void HandleHover(GameState game, Tile<TileInfo> rootTile)
        {
            ghost.SetRootTile(rootTile);
        }

        public void HandleClick(GameState game, Tile<TileInfo> rootTile)
        {
            if (this.OccupiedTiles(rootTile).Any(t => !t.IsValid || !t.Info.IsPassable))
                return;

            game.Add(new PlayerBuilding(blueprint, rootTile));
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
