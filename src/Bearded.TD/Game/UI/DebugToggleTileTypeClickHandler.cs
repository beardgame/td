using System.Linq;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.UI
{
    class DebugToggleTileTypeClickHandler : IClickHandler
    {
        public TileSelection Selection => TileSelection.Single;

        public void HandleHover(GameState game, PositionedFootprint footprint)
        { }

        public void HandleClick(GameState game, PositionedFootprint footprint)
        {
            footprint.OccupiedTiles
                .Where(t => t.IsValid)
                .ForEach(tile => game.Geometry.ToggleTileType(tile));
        }

        public void Enable(GameState game)
        { }

        public void Disable(GameState game)
        { }
    }
}
