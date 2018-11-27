using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Input
{
    class DebugSetTileTypeInteractionHandler : InteractionHandler
    {
        private readonly TileGeometry.TileType tileType;

        public DebugSetTileTypeInteractionHandler(GameInstance game, TileGeometry.TileType tileType)
            : base(game)
        {
            this.tileType = tileType;
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            if (cursor.Click.Hit)
                cursor.CurrentFootprint.OccupiedTiles
                    .Where(t => Game.State.Level.IsValid(t))
                    .ForEach(setTile);
            else if (cursor.Cancel.Hit)
                Game.PlayerInput.ResetInteractionHandler();
        }
        
        private void setTile(Tile tile) => Game.Request(SetTileType.Request, tile, tileType, TileDrawInfo.For(tileType));
    }
}
