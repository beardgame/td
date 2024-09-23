using System;
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
    private Vector3 getNormalAt(Vector3 p)
    {
        const float delta = 0.01f;

        var gradient = new Vector3(
            (getDistanceAt((p.X + delta, p.Y, p.Z)) - getDistanceAt((p.X - delta, p.Y, p.Z))) / (2 * delta),
            (getDistanceAt((p.X, p.Y + delta, p.Z)) - getDistanceAt((p.X, p.Y - delta, p.Z))) / (2 * delta),
            (getDistanceAt((p.X, p.Y, p.Z + delta)) - getDistanceAt((p.X, p.Y, p.Z - delta))) / (2 * delta)
        );

        return gradient.Normalized();
    }

    private float getDistanceAt(Vector3 p)
    {
        var (wx, wy, wz) = (Vector3i)(p * 100);

        var noise =
                +(Noise.CalcPixel3D(wx, wy, wz, 0.0005f) / 256 - 0.5f) * 0f
                + (Noise.CalcPixel3D(wx, wy, wz, 0.005f) / 256 - 0.5f) * 0.5f
                + (Noise.CalcPixel3D(wx, wy, wz, 0.05f) / 256 - 0.5f) * 0.1f
            ;

        var d = getLevelGeometryDistanceAt(new Position3(p));

        var n = (-p.Z / 3).Clamped(0, 1);

        return d;
    }

    private TileSelection triangleTileSelection1 = TileSelection.FromFootprint(
        new Footprint(ModAwareId.Invalid, [new Step(0, 0), new Step(1, 0), new Step(0, 1)])
    );

    private TileSelection triangleTileSelection2 = TileSelection.FromFootprint(
        new Footprint(ModAwareId.Invalid, [new Step(0, 0), new Step(1, 0), new Step(1, -1)])
    );

    private float getLevelGeometryDistanceAt(Position3 worldPosition)
    {
        var geometry = game.State.GeometryLayer;
        var level = game.State.Level;

        var distance = float.MaxValue;

        var xy = worldPosition.XY();
        var footprint1 = triangleTileSelection1.GetPositionedFootprint(xy);
        var footprint2 = triangleTileSelection2.GetPositionedFootprint(xy);
        var distanceSquared1 = (footprint1.CenterPosition - xy).LengthSquared;
        var distanceSquared2 = (footprint2.CenterPosition - xy).LengthSquared;

        var tiles = distanceSquared1 < distanceSquared2 ? footprint1.OccupiedTiles : footprint2.OccupiedTiles;

        var z = worldPosition.Z.NumericValue;

        foreach (var tile in tiles)
        {
            if (!level.IsValid(tile))
                continue;

            var h = geometry[tile].DrawInfo.Height.NumericValue;
            
            var heightD = z - h;

            var tileCenter = Tiles.Level.GetPosition(tile).NumericValue;
            var relativePosition = xy.NumericValue - tileCenter;

            var hexRadiusAtZ = HexagonSide;

            if (z > 0)
            {
                //hexRadiusAtZ -= z * 0.2f;
            }
            else
            {
                //hexRadiusAtZ -= z * 0.05f;
            }

            var hexD = hexDistance(relativePosition, hexRadiusAtZ, 0, 0, 0);

            var tileD = intersect(hexD, heightD);

            distance = union(distance, tileD);
        }


        return distance;
    }

    private static float union(float a, float b) => Math.Min(a, b);
    private static float intersect(float a, float b) => Math.Max(a, b);

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
