using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Game.Input
{
    interface ICursorHandler
    {
        ActionState Click { get; }
        ActionState Cancel { get; }
        PositionedFootprint CurrentFootprint { get; }
        void Update(UpdateEventArgs args, InputState inputContext);
        void SetTileSelection(TileSelection tileSelection);
    }
}
