using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.UI.Input
{
    class DebugSetTileTypeInteractionHandler : InteractionHandler
    {
        private readonly TileInfo.Type tileType;

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
        
        private void setTile(Tile<TileInfo> tile) => Game.Request(SetTileType.Request, tile, tileType, TileDrawInfo.For(tileType));
    }
}
