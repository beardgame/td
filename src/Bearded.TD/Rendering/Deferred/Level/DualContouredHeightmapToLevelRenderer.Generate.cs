using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred.Level;


sealed partial class DualContouredHeightmapToLevelRenderer
{
    private const int C000 = 0;
    private const int C100 = 1;
    private const int C010 = 2;
    private const int C001 = 3;
    private const int C110 = 4;
    private const int C101 = 5;
    private const int C011 = 6;
    private const int C111 = 7;

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

    private static readonly (Vector3i V0, Vector3i V1)[] cubeEdges =
    [
        (cube[C000], cube[C100]),
        (cube[C010], cube[C110]),
        (cube[C001], cube[C101]),
        (cube[C011], cube[C111]),

        (cube[C000], cube[C010]),
        (cube[C100], cube[C110]),
        (cube[C001], cube[C011]),
        (cube[C101], cube[C111]),

        (cube[C000], cube[C001]),
        (cube[C100], cube[C101]),
        (cube[C010], cube[C011]),
        (cube[C110], cube[C111]),
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
        var changingEdges = findChangingEdges(subdivisionEdges, distanceField);

        var vertexIndices = createVertices(subdivisionVertices, distanceField, changingEdges, vertices, boundingBox, subdivisionStep);

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

                    distanceField[x, y, z] = getDistanceAt(worldPosition);
                }
            }
        }

        return distanceField;
    }

    private static Dictionary<(Vector3i, Vector3i), float> findChangingEdges(
        Vector3i subdivisionEdges,
        float[,,] distanceField)
    {
        var edges = new Dictionary<(Vector3i, Vector3i), float>();

        var subdivisionVertices = subdivisionEdges - new Vector3i(1, 1, 1);

        for (var x = 0; x < subdivisionEdges.X; x++)
        {
            for (var y = 0; y < subdivisionEdges.Y; y++)
            {
                for (var z = 0; z < subdivisionEdges.Z; z++)
                {
                    if (x < subdivisionVertices.X)
                    {
                        considerEdge(x, y, z, 1, 0, 0);
                    }
                    if (y < subdivisionVertices.Y)
                    {
                        considerEdge(x, y, z, 0, 1, 0);
                    }
                    if (z < subdivisionVertices.Z)
                    {
                        considerEdge(x, y, z, 0, 0, 1);
                    }
                }
            }
        }

        return edges;

        void considerEdge(int x, int y, int z, int dx, int dy, int dz)
        {
            var d0 = distanceField[x, y, z];
            var d1 = distanceField[x + dx, y + dy, z + dz];
            if (d0 > 0 != d1 > 0)
            {
                addEdge((x, y, z), (dx, dy, dz), findIntersectionWith0(d0, d1));
            }
        }

        void addEdge(Vector3i v0, Vector3i dv, float f)
        {
            edges.Add((v0, v0 + dv), f);
        }
    }

    private static float findIntersectionWith0(float d0, float d1)
    {
        return d0 / (d0 - d1);
    }

    private Dictionary<Vector3i, ushort> createVertices(Vector3i subdivisionVertices,
        float[,,] distanceField,
        Dictionary<(Vector3i, Vector3i), float> changingEdges,
        BufferStream<LevelVertex> vertices,
        Box3 boundingBox,
        Vector3 subdivisionStep)
    {
        var vertexIndices = new Dictionary<Vector3i, ushort>();

        var vertexEdgeChanges = new List<(Vector3 P, Vector3 N)>(12);

        for (var x = 0; x < subdivisionVertices.X; x++)
        {
            for (var y = 0; y < subdivisionVertices.Y; y++)
            {
                for (var z = 0; z < subdivisionVertices.Z; z++)
                {
                    var p0 = new Vector3i(x, y, z);

                    vertexEdgeChanges.Clear();

                    foreach (var edge in cubeEdges)
                    {
                        var v0 = p0 + edge.V0;
                        var v1 = p0 + edge.V1;

                        if (!changingEdges.TryGetValue((v0, v1), out var f))
                            continue;

                        var p = new Vector3(edge.V0) * (1 - f) + new Vector3(edge.V1) * f;
                        var n = getNormalAt(p0 + p);

                        vertexEdgeChanges.Add((p, n));
                    }

                    if (vertexEdgeChanges.Count <= 1)
                    {
                        continue;
                    }

                    var averagePoint = Vector3.Zero;
                    foreach (var (p, n) in vertexEdgeChanges)
                    {
                        averagePoint += p;
                    }
                    averagePoint /= vertexEdgeChanges.Count;

                    var point = boundingBox.Min + (p0 + averagePoint) * subdivisionStep;
                    var normal = getNormalAt(point);

                    vertexIndices[p0] = (ushort)vertices.Count;
                    vertices.Add(new LevelVertex(point, normal, Vector2.Zero, Color.White));
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
                    v0, v2, v1,
                    v1, v2, v3,
                ]);
            }
            else
            {
                indices.Add([
                    v0, v1, v2,
                    v1, v3, v2,
                ]);
            }
        }
    }

}
