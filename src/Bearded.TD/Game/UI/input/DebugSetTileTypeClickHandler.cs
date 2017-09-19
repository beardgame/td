using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.UI
{
    class DebugSetTileTypeInteractionHandler : InteractionHandler
    {
        private readonly TileInfo.Type tileType;

        public TileSelection Selection => TileSelection.FromFootprints(FootprintGroup.Single);

        public DebugSetTileTypeInteractionHandler(GameInstance game, TileInfo.Type tileType)
            : base(game)
        {
            this.tileType = tileType;
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            if (cursor.Click.Hit)
                cursor.CurrentFootprint.OccupiedTiles
                    .Where(t => t.IsValid)
                    .ForEach(setTile);
        }
        
        private void setTile(Tile<TileInfo> tile) => Game.Request(SetTileType.Request, tile, tileType);
    }
}
