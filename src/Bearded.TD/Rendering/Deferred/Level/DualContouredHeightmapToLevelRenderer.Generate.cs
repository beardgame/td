using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
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
        var changingEdges = findChangingEdges(subdivisionEdges, distanceField, boundingBox, subdivisionStep);

        var vertexList = new List<LevelVertex>();

        var vertexIndices = createVertices(subdivisionVertices, distanceField, changingEdges, vertexList, boundingBox,
            subdivisionStep);

        if (vertexList.Count == 0)
        {
            cell.QueueFlushOnNextRender();
            return;
        }

        vertices.Add(CollectionsMarshal.AsSpan(vertexList));

        fillIndexBuffer(indices, vertexIndices, vertexList, subdivisionVertices, distanceField);

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

    private Dictionary<(Vector3i, Vector3i), float> findChangingEdges(
        Vector3i subdivisionEdges,
        float[,,] distanceField,
        Box3 boundingBox, 
        Vector3 subdivisionStep)
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
                var v0 = boundingBox.Min + (x, y, z) * subdivisionStep;
                var vd = new Vector3(dx, dy, dz) * subdivisionStep;

                var intersection = findIntersectionWith0(d0, d1);
                var currentDistance = Math.Abs(getDistanceAt(v0 + vd * intersection));

                var steps = 20;

                for (var i = 0; i <= 20; i++)
                {
                    var candidate = i / (float)steps;

                    var d = Math.Abs(getDistanceAt(v0 + vd * candidate));

                    if (d < currentDistance)
                    {
                        currentDistance = d;
                        intersection = candidate;
                    }
                }

                addEdge((x, y, z), (dx, dy, dz), intersection);
            }
        }

        void addEdge(Vector3i v0, Vector3i dv, float f)
        {
            edges.Add((v0, v0 + dv), f);
        }
    }

    private static float findIntersectionWith0(float d0, float d1)
    {
        DebugAssert.State.Satisfies(d0 > 0 != d1 > 0);
        return d0 / (d0 - d1);
    }

    private Dictionary<Vector3i, ushort> createVertices(Vector3i subdivisionVertices,
        float[,,] distanceField,
        Dictionary<(Vector3i, Vector3i), float> changingEdges,
        List<LevelVertex> vertices,
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
                    var min = boundingBox.Min + p0 * subdivisionStep;
                    var max = min + subdivisionStep;

                    vertexEdgeChanges.Clear();

                    foreach (var edge in cubeEdges)
                    {
                        var v0 = p0 + edge.V0;
                        var v1 = p0 + edge.V1;

                        if (!changingEdges.TryGetValue((v0, v1), out var f))
                            continue;

                        var pLocal = new Vector3(edge.V0) * (1 - f) + new Vector3(edge.V1) * f;
                        var p = min + pLocal * subdivisionStep;
                        var n = tryGetNormalAt(p);

                        if (n is { } nn)
                        {
                            vertexEdgeChanges.Add((p, nn));
                        }
                    }

                    if (vertexEdgeChanges.Count <= 2)
                    {
                        continue;
                    }

                    var meanPoint =
                        vertexEdgeChanges.Aggregate(Vector3.Zero, (v, t) => v + t.P) / vertexEdgeChanges.Count;

                    meanPoint = new Vector3(
                        meanPoint.X.Clamped(min.X, max.X),
                        meanPoint.Y.Clamped(min.Y, max.Y),
                        meanPoint.Z.Clamped(min.Z, max.Z)
                    );

                    var bias = 0.01f;
                    vertexEdgeChanges.Add((meanPoint, (bias, 0, 0)));
                    vertexEdgeChanges.Add((meanPoint, (0, bias, 0)));
                    vertexEdgeChanges.Add((meanPoint, (0, 0, bias)));

                    var solution = meanPoint;

                    try
                    {
                        solution = PlaneIntersectionSolver.FindIntersectionGradientDescent(vertexEdgeChanges, meanPoint, boundingBox.Min + p0 * subdivisionStep, subdivisionStep);
                    }
                    catch
                    {
                        solution = PlaneIntersectionSolver.FindIntersectionBruteForce(vertexEdgeChanges, meanPoint, boundingBox.Min + p0 * subdivisionStep, subdivisionStep);
                    }

                    var point = solution;
                    var maybeNormal = tryGetNormalAt(point);

                    if (maybeNormal is { } normal)
                    {
                        vertexIndices[p0] = (ushort)vertices.Count;
                        vertices.Add(new LevelVertex(point, normal, Vector2.Zero, Color.White));
                    }
                }
            }
        }

        return vertexIndices;
    }

    private static void fillIndexBuffer(BufferStream<ushort> bufferStream,
        Dictionary<Vector3i, ushort> vertexIndices,
        List<LevelVertex> vertices,
        Vector3i subdivisionVertices,
        float[,,] distanceField)
    {
        for (var x = 0; x < subdivisionVertices.X - 1; x++)
        {
            for (var y = 0; y < subdivisionVertices.Y - 1; y++)
            {
                for (var z = 0; z < subdivisionVertices.Z - 1; z++)
                {
                    var p0 = new Vector3i(x, y, z);
                    tryAddQuad(p0, (1, 0, 0), (0, 1, 0), vertices, vertexIndices, bufferStream, distanceField);
                    tryAddQuad(p0, (0, 0, 1), (1, 0, 0), vertices, vertexIndices, bufferStream, distanceField);
                    tryAddQuad(p0, (0, 1, 0), (0, 0, 1), vertices, vertexIndices, bufferStream, distanceField);
                }
            }
        }

        return;

        static void tryAddQuad(Vector3i p0, Vector3i tangent1, Vector3i tangent2, List<LevelVertex> vertices,
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

            if (!(
                    vertexIndices.TryGetValue(p0, out var v0) &&
                    vertexIndices.TryGetValue(p0 + tangent1, out var v1) &&
                    vertexIndices.TryGetValue(p0 + tangent2, out var v2) &&
                    vertexIndices.TryGetValue(p0 + tangent1 + tangent2, out var v3)
                ))
            {
                return;
            }

            // ensure front face is facing out
            if (solid1)
            {
                (v1, v2) = (v2, v1);
            }

            var d03 = vertices[v0].Position - vertices[v3].Position;
            var d12 = vertices[v1].Position - vertices[v2].Position;

            // connect shorter diagonal, which tends to lead to smoother mesh
            if (d12.LengthSquared < d03.LengthSquared)
            {
                (v0, v1, v3, v2) = (v1, v3, v2, v0);
            }

            // diagonal from v0 to v3
            indices.Add([
                v0, v1, v3,
                v0, v3, v2,
            ]);
        }
    }
}

public class PlaneIntersectionSolver
{
    public static Vector3 FindIntersectionBruteForce(
        List<(Vector3 P, Vector3 N)> planes,
        Vector3 guess,
        Vector3 boundingBoxMin,
        Vector3 subdivisionStep)
    {
        var constant = (Span<float>)stackalloc float[planes.Count];

        var guessError = 0f;
        for (var i = 0; i < planes.Count; i++)
        {
            constant[i] = Vector3.Dot(planes[i].N, planes[i].P);
            guessError += Math.Abs(Vector3.Dot(planes[i].N, guess) - constant[i]);
        }

        var best = guess;
        var bestError = guessError;
        for (var x = 0.0f; x < 1; x += 0.1f)
        {
            for (var y = 0.0f; y < 1; y += 0.1f)
            {
                for (var z = 0.0f; z < 1; z += 0.1f)
                {
                    var error = 0.0f;
                    var candidate = boundingBoxMin + new Vector3(x, y, z) * subdivisionStep;

                    for (var i = 0; i < planes.Count; i++)
                    {
                        var n = planes[i].N;
                        error += Math.Abs(Vector3.Dot(n, candidate) - constant[i]);
                    }

                    if (error < bestError)
                    {
                        bestError = error;
                        best = candidate;
                    }
                }
            }
        }

        return best;
    }

    public static Vector3 FindIntersectionGradientDescent(
        List<(Vector3 P, Vector3 N)> planes,
        Vector3 guess,
        Vector3 min,
        Vector3 size)
    {
        var m = planes.Count;

        var A = Matrix<double>.Build.Dense(m, 3);
        var b = Vector<double>.Build.Dense(m);

        for (var i = 0; i < m; i++)
        {
            var n = planes[i].N;
            var p = planes[i].P;
            A.SetRow(i, (double[])[n.X, n.Y, n.Z]);
            b[i] = Vector3.Dot(n, p);
        }

        Func<Vector<double>, double> objectiveFunction = x =>
        {
            var diff = A * x - b;
            return diff.DotProduct(diff);
        };

        Func<Vector<double>, Vector<double>> gradientFunction = x => 2 * A.TransposeThisAndMultiply(A * x - b);

        var objective = ObjectiveFunction.Gradient(objectiveFunction, gradientFunction);

        var initialGuess = Vector<double>.Build.Dense([guess.X, guess.Y, guess.Z]);

        var lowerBound = Vector<double>.Build.Dense([min.X, min.Y, min.Z]);
        var upperBound = Vector<double>.Build.Dense([min.X + size.X, min.Y + size.Y, min.Z + size.Z]);

        var minimizer = new BfgsBMinimizer(
            gradientTolerance: 1e-2,
            parameterTolerance: 1e-5,
            functionProgressTolerance: 1,
            maximumIterations: 100);

        var result = minimizer.FindMinimum(objective, lowerBound, upperBound, initialGuess);

        var v = result.MinimizingPoint;
        return new Vector3((float)v[0], (float)v[1], (float)v[2]);
    }
}
