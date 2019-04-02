using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.IO;
using OpenTK;

namespace Bearded.TD.Game.Generation
{
    sealed class PerlinTilemapGenerator : ITilemapGenerator
    {
        private const int hardnessRampDistance = 5;

        private readonly Logger logger;

        public PerlinTilemapGenerator(Logger logger)
        {
            this.logger = logger;
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var typeTilemap = new Tilemap<TileType>(radius, _ => TileType.Wall);
            var hardnessTilemap = new Tilemap<double>(radius);
            var gen = new Generator(typeTilemap, hardnessTilemap, seed, logger);

            gen.GenerateTilemap();

            logger.Trace?.Log("Copying tilemap data into final tilemap.");

            var tilemap = new Tilemap<TileGeometry>(radius);
            foreach (var t in tilemap)
            {
                tilemap[t] = new TileGeometry(typeTilemap[t], hardnessTilemap[t]);
            }

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return tilemap;
        }

        private class Generator
        {
            private readonly Tilemap<TileType> typeTilemap;
            private readonly Tilemap<double> hardnessTilemap;
            private readonly Logger logger;
            private readonly Level level;
            private readonly Random random;
            private readonly PerlinSourcemapGenerator perlinSourcemapGenerator;

            public Generator(
                Tilemap<TileType> typeTilemap,
                Tilemap<double> hardnessTilemap,
                int seed,
                Logger logger)
            {
                this.typeTilemap = typeTilemap;
                this.hardnessTilemap = hardnessTilemap;
                this.logger = logger;
                level = new Level(typeTilemap.Radius);
                random = new Random(seed);
                perlinSourcemapGenerator = new PerlinSourcemapGenerator(random);
            }

            public void GenerateTilemap()
            {
                var sourceMap1 = new Tilemap<double>(hardnessTilemap.Radius);
                var sourceMap2 = new Tilemap<double>(hardnessTilemap.Radius);
                var sourceMap3 = new Tilemap<double>(hardnessTilemap.Radius);

                perlinSourcemapGenerator.FillTilemapWithPerlinNoise(
                    sourceMap1, 10, (tilemap, tile) => 1.8 * Math.Abs(tilemap[tile.RotatedClockwiseAroundOrigin()]));
                perlinSourcemapGenerator.FillTilemapWithPerlinNoise(
                    sourceMap2, 5, (tilemap, tile) => 0.6 * Math.Abs(tilemap[tile.RotatedCounterClockwiseAroundOrigin()]));
                // Keep the lowest amplitude noise coordinate-system aligned
                perlinSourcemapGenerator.FillTilemapWithPerlinNoise(
                    sourceMap3, 3, (tilemap, tile) => 0.2 * Math.Abs(tilemap[tile]));

                foreach (var tile in hardnessTilemap)
                {
                    double hardnessOverride = 0;
                    var distanceFromEdge = hardnessTilemap.Radius - tile.Radius;

                    if (distanceFromEdge < hardnessRampDistance)
                    {
                        hardnessOverride = 1 - (double) distanceFromEdge / hardnessRampDistance;
                    }

                    var desiredHardness = (sourceMap1[tile] + sourceMap2[tile] + sourceMap3[tile]).Clamped(0, 1);
                    hardnessTilemap[tile] = Math.Max(desiredHardness, hardnessOverride);
                }

                createPathsFromNoiseTilemap();
                clearCenter(4);

                carve();
            }

            private void createPathsFromNoiseTilemap()
            {
                logger.Trace?.Log("Digging paths to all corners");
                var result = doAllPairPathFindingFromTile(Tile.Origin);

                var corners = Directions
                    .All
                    .Enumerate()
                    .Select(dir => Tile.Origin.Offset(dir.Step() * typeTilemap.Radius));
                foreach (var start in corners)
                {
                    var curr = start;
                    while (curr != Tile.Origin)
                    {
                        typeTilemap[curr] = TileType.Floor;
                        curr = result[curr].Parent;
                    }
                }
            }

            private Tilemap<(Tile Parent, double Cost)> doAllPairPathFindingFromTile(Tile origin)
            {
                var q = new PriorityQueue<double, Tile>();
                var result = new Tilemap<(Tile Parent, double Cost)>(hardnessTilemap.Radius);

                result.ForEach(t => result[t] = (origin, double.PositiveInfinity));

                q.Enqueue(0, origin);
                result[origin] = (origin, 0);

                while (q.Count > 0)
                {
                    var (currPriority, currTile) = q.Dequeue();

                    foreach (var neighbor in level.ValidNeighboursOf(currTile))
                    {
                        var costToNeighbor = result[neighbor].Cost;
                        var candidateCost = currPriority + hardnessTilemap[neighbor];

                        if (candidateCost >= costToNeighbor) continue;

                        result[neighbor] = (currTile, candidateCost);

                        if (double.IsPositiveInfinity(costToNeighbor))
                        {
                            q.Enqueue(candidateCost, neighbor);
                        }
                        else
                        {
                            q.DecreasePriority(neighbor, candidateCost);
                        }
                    }
                }

                return result;
            }

            private void clearCenter(int radius)
            {
                logger.Trace?.Log("Clearing center tiles");

                foreach (var tile in typeTilemap.SpiralCenteredAt(Tile.Origin, radius))
                {
                    typeTilemap[tile] = TileType.Floor;
                }
            }

            private void carve()
            {
                carveEverythingBelowHardness(0.08);

                var q = new Queue<Tile>(typeTilemap.Where(isType(TileType.Floor)));

                while (q.Count > 0)
                {
                    var curr = q.Dequeue();
                    foreach (var neighbor in level.ValidNeighboursOf(curr).Where(isType(TileType.Wall)))
                    {
                        if (random.NormalDouble(0, 0.33) >= hardnessTilemap[curr])
                        {
                            var type = typeTilemap[curr];
                            typeTilemap[neighbor] = type;
                            q.Enqueue(neighbor);
                        }
                    }
                }

                Func<Tile, bool> isType(TileType type) => tile => typeTilemap[tile] == type;
            }

            private void carveEverythingBelowHardness(double hardness)
            {
                foreach (var tile in hardnessTilemap.Where(tile =>
                    typeTilemap[tile] == TileType.Wall && hardnessTilemap[tile] < hardness))
                {
                    typeTilemap[tile] = TileType.Floor;
                }
            }
        }

        private class PerlinSourcemapGenerator
        {
            private readonly Random random;
            private readonly Vector2[] vectors;

            public PerlinSourcemapGenerator(Random random)
            {
                this.random = random;

                // Use uniformly distributed vectors to force better gradient distribution.
                const int numSamples = 12;
                vectors = Enumerable.Range(0, numSamples)
                    .Select(i => Direction2.FromRadians(Mathf.TwoPi * i / numSamples).Vector)
                    .ToArray();
            }

            public void FillTilemapWithPerlinNoise(
                Tilemap<double> tilemap, int gridSize, Func<Tilemap<double>, Tile, double> noiseTransformer)
            {
                var maxDeviationFromCenter = (tilemap.Radius + 1) * Constants.Game.World.HexagonDiameter;

                var gradientArrayDimension = Mathf.CeilToInt(2 * maxDeviationFromCenter / gridSize) + 1;
                var gradientArray = createRandomGradientGrid(gradientArrayDimension);

                var sourceMap = new Tilemap<double>(tilemap.Radius);

                fillTilemapWithPerlin(sourceMap, gridSize, gradientArray);

                foreach (var t in tilemap)
                {
                    tilemap[t] = noiseTransformer(sourceMap, t);
                }
            }

            private Vector2[,] createRandomGradientGrid(int dimension)
            {
                var grid = new Vector2[dimension, dimension];
                for (var j = 0; j < dimension; j++)
                {
                    for (var i = 0; i < dimension; i++)
                    {
                        grid[i, j] = vectors[random.Next(vectors.Length)];
                    }
                }

                return grid;
            }

            // Fills a tilemap with values 0-1 based on the Perlin noise gradient array.
            private static void fillTilemapWithPerlin(
                Tilemap<double> tilemap, int gridSize, Vector2[,] gradientArray)
            {
                foreach (var tile in tilemap)
                {
                    tilemap[tile] = perlinAtGridCellFromWorldSpace(gridSize, gradientArray, tile);
                }
            }

            private float perlinAtGridCellFromTileSpace(
                Tilemap<double> tilemap, int gridSize, Vector2[,] gradientArray, Tile tile)
            {
                var xInGradientArraySpace = (float) (tile.X + tilemap.Radius) / gridSize;
                var yInGradientArraySpace = (float) (tile.Y + tilemap.Radius) / gridSize;

                return perlinAt(gradientArray, xInGradientArraySpace, yInGradientArraySpace);
            }

            private static float perlinAtGridCellFromWorldSpace(int gridSize, Vector2[,] gradientArray, Tile tile)
            {
                var offset = new Vector2(gradientArray.GetLength(0) / 2f, gradientArray.GetLength(1) / 2f);

                var worldPosition = Level.GetPosition(tile).NumericValue / gridSize + offset;

                return perlinAt(gradientArray, worldPosition.X, worldPosition.Y);
            }

            private static float perlinAt(Vector2[,] gradientArray, float x, float y)
            {
                // Grid coordinates lower and upper
                var xLower = (int) x;
                var xUpper = xLower + 1;

                var yLower = (int) y;
                var yUpper = yLower + 1;

                // Calculate dot products between distance and gradient for each of the grid corners
                var topLeft = dotProductWithGridDirection(
                    gradientArray, xLower, yUpper, x, y);
                var topRight = dotProductWithGridDirection(
                    gradientArray, xUpper, yUpper, x, y);
                var bottomLeft = dotProductWithGridDirection(
                    gradientArray, xLower, yLower, x, y);
                var bottomRight = dotProductWithGridDirection(
                    gradientArray, xUpper, yLower, x, y);

                // Interpolation weights
                var tx = Interpolate.SmoothStep(0f, 1f, x - xLower);
                var ty = 1 - Interpolate.SmoothStep(0f, 1f, y - yLower);

                var top = Interpolate.Lerp(topLeft, topRight, tx);
                var bottom = Interpolate.Lerp(bottomLeft, bottomRight, tx);
                return Interpolate.Lerp(top, bottom, ty);
            }

            private static float dotProductWithGridDirection(
                Vector2[,] gradientArray, int gridX, int gridY, float x, float y)
            {
                var distance = new Vector2(x, y) - new Vector2(gridX, gridY);
                return Vector2.Dot(distance, gradientArray[gridX, gridY]);
            }
        }
    }
}
