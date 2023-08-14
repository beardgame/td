using System;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Generation.ObjectPositioning.AlignmentMode;
using static Bearded.TD.Game.Generation.ObjectPositioning.RotationMode;

namespace Bearded.TD.Game.Generation;

static class ObjectPositioning
{
    public enum AlignmentMode
    {
        CenterOfTile = 0,
        RandomlyInTile,
    }

    public enum RotationMode
    {
        FixedDirection = 0,
        RandomDirection,
    }

    public static Position2 ToPosition(this AlignmentMode mode, Tile tile, Random random)
    {
        return mode switch
        {
            CenterOfTile => Level.GetPosition(tile),
            RandomlyInTile => Level.GetPosition(tile) +
                GeometricRandom.UniformRandomPointOnDisk(random, Constants.Game.World.HexagonInnerRadius),
            _ => throw new ArgumentOutOfRangeException($"Unhandled alignment mode: {mode}")
        };
    }

    public static Direction2 ToDirection(this RotationMode mode, Random random)
    {
        return mode switch
        {
            FixedDirection => Direction2.Zero,
            RandomDirection => Direction2.FromDegrees(random.NextSingle() * 360),
            _ => throw new ArgumentOutOfRangeException($"Unhandled rotation mode: {mode}")
        };
    }
}
