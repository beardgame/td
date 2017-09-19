using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Game.UI
{
    interface ICursorHandler
    {
        IAction ClickAction { get; }
        PositionedFootprint CurrentFootprint { get; }
        void Update(UpdateEventArgs args, InputState inputContext);
        void SetTileSelection(TileSelection tileSelection);
    }
}
