using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using SimplexNoise;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed partial class DualContouredHeightmapToLevelRenderer
{
    private static readonly Vector3i[] cube =
    [
        (0, 0, 0),
        (1, 0, 0),
        (0, 1, 0),
        (0, 0, 1),

        (1, 1, 0),
        (1, 0, 1),
        (0, 1, 1),
        (1, 1, 1),
    ];

    private void fillCellBuffers(Cell cell)
    {
        var vertices = cell.Vertices;
        var indices = cell.Indices;
        var boundingBox = cell.BoundingBox;
        var subdivision = cell.Subdivision;

        vertices.Clear();
        indices.Clear();

        var subdivisionStep = boundingBox.Size / (subdivision - new Vector3(1, 1, 1));

        var subdivisionEdges = subdivision + new Vector3i(1, 1, 1);
        var subdivisionVertices = subdivision;

        var distanceField = fillDistanceField(subdivisionEdges, boundingBox, subdivisionStep);

        var vertexIndices = createVertices(subdivisionVertices, distanceField, vertices, boundingBox, subdivisionStep);

        if (vertices.Count == 0)
        {
            cell.QueueFlushOnNextRender();
            return;
        }

        fillIndexBuffer(indices, vertexIndices, subdivisionVertices, distanceField);

        cell.QueueFlushOnNextRender();
    }

    private float[,,] fillDistanceField(
        Vector3i subdivisionEdges, Box3 boundingBox, Vector3 subdivisionStep)
    {
        var geometry = game.State.GeometryLayer;
        var level = game.State.Level;

        var distanceField = new float[subdivisionEdges.X, subdivisionEdges.Y, subdivisionEdges.Z];

        for (var x = 0; x < subdivisionEdges.X; x++)
        {
            for (var y = 0; y < subdivisionEdges.Y; y++)
            {
                var worldXY = boundingBox.Min.Xy + subdivisionStep.Xy * (x, y);

                var tile = Tiles.Level.GetTile(new Position2(worldXY));

                if (!level.IsValid(tile))
                    continue;

                for (var z = 0; z < subdivisionEdges.Z; z++)
                {
                    var worldZ = boundingBox.Min.Z + subdivisionStep.Z * z;
                    
                    var worldPosition = worldXY.WithZ(worldZ);
                    
                    var (wx, wy, wz) = (Vector3i)(worldPosition * 100);

                    var noise =
                        + (Noise.CalcPixel3D(wx, wy, wz, 0.0005f) / 256 - 0.5f) * 0f
                        + (Noise.CalcPixel3D(wx, wy, wz, 0.005f) / 256 - 0.5f) * 0.5f
                        + (Noise.CalcPixel3D(wx, wy, wz, 0.05f) / 256 - 0.5f) * 0.1f
                        ;

                    var d = getLevelGeometryDistanceAt(new Position3(worldPosition));

                    var n = (-worldZ / 3).Clamped(0, 1);

                    distanceField[x, y, z] = d;//noise * n + d * (1 - n);
                }
            }
        }

        return distanceField;
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

        var thisTile = Tiles.Level.GetTile(xy);

        foreach (var tile in thisTile.PossibleNeighbours().Append(thisTile))
        {
            if (!level.IsValid(tile))
                continue;

            var h = geometry[tile].DrawInfo.Height.NumericValue;
            var hd = h - worldPosition.Z.NumericValue;

            var tileCenter = Tiles.Level.GetPosition(tile).NumericValue;
            
            var hexRadiusAtZ = Math.Abs(h) * 0.5f - Constants.Game.World.HexagonSide;
            var relativePosition = xy.NumericValue - tileCenter;
            var cornerRadius = 0f;
            var r = Math.Max(hexRadiusAtZ - cornerRadius, 0);
            var d2 = new Vector2(Math.Abs(relativePosition.Y), Math.Abs(relativePosition.X));
            d2 -= 2 * Math.Min(Vector2.Dot(d2, (-0.866025404f, 0.5f)), 0) * new Vector2(-0.866025404f, 0.5f);
            d2 -= new Vector2(Math.Clamp(d2.X, -0.577350269f * r, 0.577350269f * r), r);
            var hexDistance = d2.Length * Math.Sign(d2.Y) - cornerRadius;

            var coneDistance = Math.Abs(hd) * -relativePosition.Length;

            var tileDistance = Math.Max(hd, coneDistance);

            distance = Math.Min(distance, tileDistance);
        }


        return distance;
    }

    private static Dictionary<Vector3i, ushort> createVertices(
        Vector3i subdivisionVertices,
        float[,,] distanceField,
        BufferStream<LevelVertex> vertices,
        Box3 boundingBox,
        Vector3 subdivisionStep)
    {
        var vertexIndices = new Dictionary<Vector3i, ushort>();

        for (var x = 0; x < subdivisionVertices.X; x++)
        {
            for (var y = 0; y < subdivisionVertices.Y; y++)
            {
                for (var z = 0; z < subdivisionVertices.Z; z++)
                {
                    var insideCount = 0;
                    var outsideCount = 0;

                    var insideSum = Vector3.Zero;
                    var outsideSum = Vector3.Zero;

                    var insideDistanceSum = 0f;
                    var outsideDistanceSum = 0f;

                    var p0 = new Vector3i(x, y, z);

                    for (var j = 0; j < 8; j++)
                    {
                        var p = p0 + cube[j];
                        var d = distanceField[p.X, p.Y, p.Z];
                        var isInside = d <= 0;
                        if (isInside)
                        {
                            insideCount++;
                            insideSum += new Vector3(cube[j]) * -d;
                            insideDistanceSum += -d;
                        }
                        else
                        {
                            outsideCount++;
                            outsideSum += new Vector3(cube[j]) * d;
                            outsideDistanceSum += d;
                        }
                    }

                    if (insideCount == 0 || outsideCount == 0)
                    {
                        continue;
                    }

                    vertexIndices[p0] = (ushort)vertices.Count;

                    var insideAverage = insideSum / insideDistanceSum;
                    var outsideAverage = outsideSum / outsideDistanceSum;

                    var gradient = (insideAverage - outsideAverage).Normalized();

                    var point = boundingBox.Min + (p0 + insideAverage) * subdivisionStep;

                    vertices.Add(new LevelVertex(point, gradient, Vector2.Zero, Color.White));
                }
            }
        }

        return vertexIndices;
    }

    private static void fillIndexBuffer(
        BufferStream<ushort> bufferStream,
        Dictionary<Vector3i, ushort> vertexIndices,
        Vector3i subdivisionVertices,
        float[,,] distanceField)
    {
        for (var x = 0; x < subdivisionVertices.X-1; x++)
        {
            for (var y = 0; y < subdivisionVertices.Y-1; y++)
            {
                for (var z = 0; z < subdivisionVertices.Z-1; z++)
                {
                    var p0 = new Vector3i(x, y, z);
                    tryAddQuad(p0, (1, 0, 0), (0, 1, 0), vertexIndices, bufferStream, distanceField);
                    tryAddQuad(p0, (0, 0, 1), (1, 0, 0), vertexIndices, bufferStream, distanceField);
                    tryAddQuad(p0, (0, 1, 0), (0, 0, 1), vertexIndices, bufferStream, distanceField);
                }
            }
        }
        return;

        static void tryAddQuad(Vector3i p0, Vector3i tangent1, Vector3i tangent2,
            Dictionary<Vector3i, ushort> vertexIndices, BufferStream<ushort> indices, float[,,] distanceField)
        {
            var d1 = p0 + tangent1 + tangent2;
            var d2 = p0 + (1, 1, 1);
            var solid1 = distanceField[d1.X, d1.Y, d1.Z] > 0;
            var solid2 = distanceField[d2.X, d2.Y, d2.Z] > 0;

            if (solid1 == solid2)
            {
                return;
            }

            var v0 = vertexIndices[p0];
            var v1 = vertexIndices[p0 + tangent1];
            var v2 = vertexIndices[p0 + tangent2];
            var v3 = vertexIndices[p0 + tangent1 + tangent2];

            if (solid1)
            {
                indices.Add([
                    v0, v1, v2,
                    v1, v3, v2,
                ]);
            }
            else
            {
                indices.Add([
                    v0, v2, v1,
                    v1, v2, v3,
                ]);
            }
        }
    }

}
