using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.UI.Input
{
    interface ICursorHandler
    {
        ActionState Click { get; }
        PositionedFootprint CurrentFootprint { get; }
        void Update(UpdateEventArgs args, InputState inputContext);
        void SetTileSelection(TileSelection tileSelection);
    }
}
