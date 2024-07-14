using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;

namespace Bearded.TD.Tiles;

static class Extensions
{
    #region Lookup Tables

    private static readonly Step[] directionDelta =
    {
        new Step(0, 0),
        new Step(1, 0),
        new Step(0, 1),
        new Step(-1, 1),
        new Step(-1, 0),
        new Step(0, -1),
        new Step(1, -1),
    };

    private static readonly Direction[] directionOpposite =
    {
        Direction.Unknown,
        Direction.Left,
        Direction.DownLeft,
        Direction.DownRight,
        Direction.Right,
        Direction.UpRight,
        Direction.UpLeft,
    };

    private static readonly Direction[] directions =
    {
        Direction.Right,
        Direction.UpRight,
        Direction.UpLeft,
        Direction.Left,
        Direction.DownLeft,
        Direction.DownRight,
    };

    private static readonly Vector2[] corners =
        Enumerable.Range(0, 7)
            .Select(i =>
                Direction2.FromDegrees(i * 60f - 30f).Vector
            )
            .ToArray();

    // ReSharper disable once InconsistentNaming
    private static readonly Direction2[] direction2s =
        Enumerable.Range(0, 7)
            .Select(i =>
                Direction2.FromDegrees((i - 1) * 60f)
            )
            .ToArray();

    private static readonly Vector2[] vectors =
        Enumerable.Range(0, 7)
            .Select(i => (i - 1) * 60 * Math.PI / 180)
            .Select(a => new Vector2((float)Math.Round(Math.Cos(a), 7), (float)Math.Round(Math.Sin(a), 7)))
            .ToArray();

    public static ImmutableArray<Direction> Directions { get; }
        = directions.ToImmutableArray();

    #endregion

    #region Tile

    public static IEnumerable<Tile> PossibleNeighbours(this Tile tile)
    {
        for (var i = 1; i <= 6; i++)
        {
            yield return tile + directionDelta[i];
        }
    }

    #endregion

    #region Direction and Directions

    public static IEnumerable<Direction> Enumerate(this Directions directions)
        => Extensions.directions.Where(direction => directions.Includes(direction));

    public static IEnumerable<Direction> EnumerateClockwise(this Directions directions, Direction start)
    {
        var offset = Math.Max(0, Array.IndexOf(Extensions.directions, start));
        for (var i = Extensions.directions.Length - 1; i >= 0; i--)
        {
            var dir = Extensions.directions[(i + offset) % Extensions.directions.Length];
            if (directions.Includes(dir)) yield return dir;
        }
    }

    public static IEnumerable<Direction> EnumerateCounterClockwise(this Directions directions, Direction start)
    {
        var offset = Math.Max(0, Array.IndexOf(Extensions.directions, start));
        for (var i = 0; i < Extensions.directions.Length; i++)
        {
            var dir = Extensions.directions[(i + offset) % Extensions.directions.Length];
            if (directions.Includes(dir)) yield return dir;
        }
    }

    public static Vector2 CornerBefore(this Direction direction) => corners[(int) direction - 1];
    public static Vector2 CornerAfter(this Direction direction) => corners[(int) direction];
    public static Direction2 SpaceTimeDirection(this Direction direction) => direction2s[(int) direction];
    public static Vector2 Vector(this Direction direction) => vectors[(int) direction];
    public static Step Step(this Direction direction) => directionDelta[(int) direction];
    public static bool Any(this Directions direction) => direction != TD.Tiles.Directions.None;
    public static bool Any(this Directions direction, Directions match) => direction.Intersect(match) != TD.Tiles.Directions.All;
    public static bool All(this Directions direction, Directions match) => direction.Intersect(match) == match;
    public static Direction Hexagonal(this Direction2 direction) => (Direction) ((int) Math.Floor(direction.Degrees * 1 / 60f + 0.5f) % 6 + 1);
    public static Direction Opposite(this Direction direction) => directionOpposite[(int) direction];
    public static Direction Next(this Direction direction) => (Direction)((int)direction % 6 + 1);
    public static Direction Previous(this Direction direction) => (Direction)(((int)direction + 4) % 6 + 1);

    public static Directions ToDirections(this Direction direction) => (Directions) (1 << ((int) direction - 1));
    public static bool Includes(this Directions directions, Direction direction) => directions.HasFlag(direction.ToDirections());
    public static Directions And(this Directions directions, Direction direction) => directions | direction.ToDirections();
    public static Directions Except(this Directions directions, Direction direction) => directions & ~direction.ToDirections();
    public static Directions Union(this Directions directions, Directions directions2) => directions | directions2;
    public static Directions Except(this Directions directions, Directions directions2) => directions & ~directions2;
    public static Directions Intersect(this Directions directions, Directions directions2) => directions & directions2;
    public static bool IsSubsetOf(this Directions directions, Directions directions2) => directions2.HasFlag(directions);

    #endregion

    #region Orientations

    public static Angle Rotation(this Orientation orientation) => (int) orientation * Angle.FromDegrees(60);
    public static Orientation HexagonalOrientation(this Direction2 direction) => (Orientation) ((int) direction.Hexagonal() - 1);

    #endregion
}
