using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.UI
{
    class DebugSetTileTypeClickHandler : IClickHandler
    {
        private readonly TileInfo.Type tileType;

        public TileSelection Selection => TileSelection.FromFootprints(FootprintGroup.Single);

        public DebugSetTileTypeClickHandler(TileInfo.Type tileType)
        {
            this.tileType = tileType;
        }

        public void HandleHover(GameInstance game, PositionedFootprint footprint)
        { }

        public void HandleClick(GameInstance game, PositionedFootprint footprint)
        {
            footprint.OccupiedTiles
                .Where(t => t.IsValid)
                .ForEach(tile => setTile(game, tile));
        }

        private void setTile(GameInstance game, Tile<TileInfo> tile)
        {
            game.Request(SetTileType.Request, tile, tileType);
        }

        public void Enable(GameInstance game)
        { }

        public void Disable(GameInstance game)
        { }
    }
}
