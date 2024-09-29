using System;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using SimplexNoise;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed partial class DualContouredHeightmapToLevelRenderer
{
    private Vector3? tryGetNormalAt(Vector3 p)
    {
        const float delta = 0.0001f;

        var p0 = getDistanceAt(new Vector3(p.X, p.Y, p.Z));

        var x1 = getDistanceAt(new Vector3(p.X + delta, p.Y, p.Z));
        var y1 = getDistanceAt(new Vector3(p.X, p.Y + delta, p.Z));
        var z1 = getDistanceAt(new Vector3(p.X, p.Y, p.Z + delta));

        var gradient = new Vector3(x1 - p0, y1 - p0, z1 - p0);

        if (!float.IsFinite(gradient.X) || !float.IsFinite(gradient.Y) || !float.IsFinite(gradient.Z))
        {
            return null;
        }

        if (gradient.LengthSquared == 0)
        {
            return null;
        }

        return gradient.Normalized();
    }

    private float getDistanceAt(Vector3 p)
    {
        var (wx, wy, wz) = (Vector3i)((p + new Vector3(1000)) * 50);

        var noise = 0
                + (Noise.CalcPixel3D(wx, wy, wz, 0.01f) / 256 - 0.5f) * 0.5f
                + (Noise.CalcPixel3D(wx, wy, wz, 0.003f) / 256 - 0.5f) * 0.5f
            ;

        var level = tryGetLevelGeometryDistanceAt(new Position3(p));

        return level.Distance + noise * (1 - level.FloorNess);
    }

    private TileSelection triangleTileSelection1 = TileSelection.FromFootprint(
        new Footprint(ModAwareId.Invalid, [new Step(0, 0), new Step(1, 0), new Step(0, 1)])
    );

    private TileSelection triangleTileSelection2 = TileSelection.FromFootprint(
        new Footprint(ModAwareId.Invalid, [new Step(0, 0), new Step(1, 0), new Step(1, -1)])
    );

    record struct LevelDistance(float Distance, float FloorNess);

    private float debugDistance1(Position3 worldPosition)
    {
        var v = worldPosition.NumericValue;

        var p = v.X * v.Y * 0.5;

        return v.Z - (float)Math.Abs(p - Math.Floor(p)) * 2 + 2 + v.Y;
    }

    private float debugDistance2(Position3 worldPosition)
    {
        var v = worldPosition.NumericValue;

        v *= 0.5f;
        v = (Vector3)new Vector3d(v.X - Math.Floor(v.X), v.Y - Math.Floor(v.Y), v.Z + 1f);

        v -= new Vector3(0.5f, 0.5f, 0f);

        var sphere = v.Length - 0.45f;
        var cut = v.X - v.Y * 0.3f + v.Z * 0.5f;

        return smoothIntersect(sphere, cut, 0.001f);
    }

    private LevelDistance tryGetLevelGeometryDistanceAt(Position3 worldPosition)
    {

        var geometry = game.State.GeometryLayer;
        var level = game.State.Level;

        var outerDistance = float.PositiveInfinity;
        var innerDistance = float.PositiveInfinity;

        var xy = worldPosition.XY();
        var footprint1 = triangleTileSelection1.GetPositionedFootprint(xy);
        var footprint2 = triangleTileSelection2.GetPositionedFootprint(xy);
        var distanceSquared1 = (footprint1.CenterPosition - xy).LengthSquared;
        var distanceSquared2 = (footprint2.CenterPosition - xy).LengthSquared;

        var tiles = distanceSquared1 < distanceSquared2 ? footprint1.OccupiedTiles : footprint2.OccupiedTiles;

        var z = worldPosition.Z.NumericValue;

        var floorNess = 0f;

        foreach (var tile in tiles)
        {
            if (!level.IsValid(tile))
                continue;

            var geo = geometry[tile];

            var h = geo.DrawInfo.Height.NumericValue;

            var isFloor = geo.Geometry.Type == TileType.Floor;

            var heightD = z - h;

            var tileCenter = Tiles.Level.GetPosition(tile).NumericValue;
            var relativePosition = xy.NumericValue - tileCenter;

            var hexRadiusAtZ = HexagonWidth * 0.55f;

            var hexD = hexDistance(relativePosition, hexRadiusAtZ, 0.2f, 0, 0);

            if (isFloor)
            {
                var impact = 1 - (hexD / HexagonSide).Clamped(0, 1);
                var nearnessToFloor = 1 - (Math.Abs(heightD) - 0.1f).Clamped(0, 1);
                floorNess = Math.Max(floorNess, impact * nearnessToFloor);
            }

            var tileD = smoothIntersect(hexD, heightD);

            if (outerDistance == float.PositiveInfinity)
            {
                outerDistance = tileD;
            }
            else
            {
                outerDistance = -smoothIntersect(-outerDistance, -tileD);
                //outerDistance = union(outerDistance, tileD);
            }
        }

        return new LevelDistance(float.IsFinite(outerDistance) ? outerDistance : z, floorNess);
    }

    private static float union(float a, float b) => Math.Min(a, b);
    private static float intersect(float a, float b) => Math.Max(a, b);

    float smoothIntersect(float a, float b, float k)
    {
        var h = (0.5f - 0.5f * (b - a) / k).Clamped(0, 1);
        var x = k * h * (1 - h);
        return Interpolate.Lerp(b, a, h) + x;
    }

    float smoothIntersect(float a, float b)
    {
        return (a + b + (float)Math.Sqrt(a * a + b * b));
    }

    private static float smoothUnion(float a, float b, float k)
    {
        var h = Math.Clamp(0.5f + 0.5f * (b - a) / k, 0, 1);
        return Interpolate.Lerp(b, a, h) - k * h * (1 - h);
    }

    private float hexDistance(Vector2 p, float radius, float cornerRadius, float innerGlowRoundness,
        float centerRoundness)
    {
        var k = new Vector3(-0.866025404f, 0.5f, 0.577350269f);
        float d = p.Length;
        float cornersToCenter = 1 - (d / radius * -k.X).Clamped(0, 1);
        float roundness = Interpolate.Lerp(innerGlowRoundness, centerRoundness, cornersToCenter);
        cornerRadius += radius * roundness * cornersToCenter;
        cornerRadius = Math.Min(cornerRadius, radius);

        float r = radius - cornerRadius;
        var d2 = new Vector2(Math.Abs(p.Y), Math.Abs(p.X));
        d2 -= 2.0f * Math.Min(Vector2.Dot(k.Xy, d2), 0) * k.Xy;
        d2 -= new Vector2(d2.X.Clamped(-k.Z * r, k.Z * r), r);
        float hexDistance = d2.Length * MathF.Sign(d2.Y);

        return hexDistance - cornerRadius;
    }
}
