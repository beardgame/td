using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    interface ICursorHandler
    {
        ActionState Click { get; }
        ActionState Cancel { get; }
        Position2 CursorPosition { get; }
        PositionedFootprint CurrentFootprint { get; }
        void Update(UpdateEventArgs args, InputState inputContext);
        void SetTileSelection(TileSelection tileSelection);
    }
}
