using System;

namespace Bearded.TD.Game.Tiles
{
    enum Direction : byte
    {
        Unknown = 0,
        Left = 1,
        DownLeft = 2,
        DownRight = 3,
        Right = 4,
        UpRight = 5,
        UpLeft = 6,
    }

    [Flags]
    enum Directions : byte
    {
        None = 0,
        Left = 1 << (Direction.Left - 1),
        DownLeft = 1 << (Direction.DownLeft - 1),
        DownRight = 1 << (Direction.DownRight - 1),
        Right = 1 << (Direction.Right - 1),
        UpRight = 1 << (Direction.UpRight - 1),
        UpLeft = 1 << (Direction.UpLeft - 1),

        All = Left | DownLeft | DownRight | Right | UpRight | UpLeft,
    }

}
