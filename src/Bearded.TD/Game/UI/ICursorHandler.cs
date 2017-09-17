using amulware.Graphics;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
{
    interface ICursorHandler
    {
        PositionedFootprint CurrentFootprint { get; }
        void Update(UpdateEventArgs args, GameInputContext inputContext);
        void SetTileSelection(TileSelection tileSelection);
    }
}
