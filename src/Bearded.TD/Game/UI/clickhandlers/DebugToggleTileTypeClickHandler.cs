using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.UI
{
    class DebugToggleTileTypeClickHandler : IClickHandler
    {
        public TileSelection Selection => TileSelection.FromFootprints(FootprintGroup.Single);

        public void HandleHover(GameInstance game, PositionedFootprint footprint)
        { }

        public void HandleClick(GameInstance game, PositionedFootprint footprint)
        {
            footprint.OccupiedTiles
                .Where(t => t.IsValid)
                .ForEach(tile => requestToggle(game, tile));
        }

        private void requestToggle(GameInstance game, Tile<TileInfo> tile)
        {
            var type = tile.Info.TileType == TileInfo.Type.Floor
                ? TileInfo.Type.Wall
                : TileInfo.Type.Floor;
            game.Request(SetTileType.Request, tile, type, TileDrawInfo.For(type));
        }

        public void Enable(GameInstance game)
        { }

        public void Disable(GameInstance game)
        { }
    }
}
