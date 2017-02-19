using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Interaction
{
    class DebugToggleTileTypeClickHandler : IClickHandler
    {
        public Footprint Footprint => Footprint.Single;

        public void HandleHover(GameState game, Tile<TileInfo> rootTile)
        { }

        public void HandleClick(GameState game, Tile<TileInfo> rootTile)
        {
            game.Geometry.ToggleTileType(rootTile);
        }

        public void Enable(GameState game)
        { }

        public void Disable(GameState game)
        { }
    }
}
