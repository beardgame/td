using System;

namespace Bearded.TD.Tiles
{
    enum Direction : byte
    {
        Unknown = 0,
        Right = 1,
        UpRight = 2,
        UpLeft = 3,
        Left = 4,
        DownLeft = 5,
        DownRight = 6,
    }

    [Flags]
    enum Directions : byte
    {
        None = 0,
        // ReSharper disable once ShiftExpressionRealShiftCountIsZero
        Right = 1 << (Direction.Right - 1),
        UpRight = 1 << (Direction.UpRight - 1),
        UpLeft = 1 << (Direction.UpLeft - 1),
        Left = 1 << (Direction.Left - 1),
        DownLeft = 1 << (Direction.DownLeft - 1),
        DownRight = 1 << (Direction.DownRight - 1),

        All = Right | UpRight | UpLeft | Left | DownLeft | DownRight,
    }

}
