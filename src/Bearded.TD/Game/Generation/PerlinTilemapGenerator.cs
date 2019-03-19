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
        private readonly Logger logger;
        private readonly int gridSize;

        public PerlinTilemapGenerator(Logger logger, int gridSize = 3)
        {
            this.logger = logger;
            this.gridSize = gridSize;
        }

        public Tilemap<TileGeometry.TileType> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var tilemap = new Tilemap<TileGeometry.TileType>(radius);
            var gen = new Generator(tilemap, seed, logger, gridSize);

            gen.GenerateTilemap();

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return tilemap;
        }

        private class Generator
        {
            private readonly Tilemap<TileGeometry.TileType> tilemap;
            private readonly Level level;
            private readonly Random random;
            private readonly Logger logger;
            private readonly int gridSize;

            public Generator(
                Tilemap<TileGeometry.TileType> tilemap,
                int seed,
                Logger logger,
                int gridSize)
            {
                this.tilemap = tilemap;
                level = new Level(tilemap.Radius);
                random = new Random(seed);
                this.logger = logger;
                this.gridSize = gridSize;
            }

            public void GenerateTilemap()
            {
                var tileArrayDimension = 2 * tilemap.Radius + 1;

                var gradientArrayDimension = Mathf.CeilToInt((double) tileArrayDimension / gridSize) + 1;
                var gradientArray = createRandomGradientGrid(gradientArrayDimension);

                var perlinTilemap = new Tilemap<double>(tilemap.Radius);
                fillTilemapWithNormalizedPerlin(gradientArray, perlinTilemap);

                resetTilemap(TileGeometry.TileType.Wall);

                createPathsFromNoiseTilemap(perlinTilemap);
                clearCenter(4);

                carve(perlinTilemap);
            }

            private Vector2[,] createRandomGradientGrid(int dimension)
            {
                logger.Trace?.Log("Generating random gradients for level generation.");

                var grid = new Vector2[dimension, dimension];
                for (var j = 0; j < dimension; j++)
                {
                    for (var i = 0; i < dimension; i++)
                    {
                        grid[i, j] = Direction2.FromRadians(random.NextFloat(0, Mathf.TwoPi)).Vector;
                    }
                }

                return grid;
            }

            // Fills a tilemap with values 0-1 based on the Perlin noise gradient array.
            private void fillTilemapWithNormalizedPerlin(Vector2[,] gradientArray, Tilemap<double> perlinTilemap)
            {
                foreach (var tile in tilemap)
                {
                    var perlin = perlinAt(gradientArray, tile.X + perlinTilemap.Radius, tile.Y + perlinTilemap.Radius);
                    var normalizedPerlin = perlin * .5 + .5;
                    perlinTilemap[tile] = normalizedPerlin;
                }
            }

            private float perlinAt(Vector2[,] gradientArray, int x, int y)
            {
                // Grid coordinates lower and upper
                var xInGradientArraySpace = (float) x / gridSize;
                var xLower = (int) xInGradientArraySpace;
                var xUpper = xLower + 1;

                var yInGradientArraySpace = (float) y / gridSize;
                var yLower = (int) yInGradientArraySpace;
                var yUpper = yLower + 1;

                // Calculate dot products between distance and gradient for each of the grid corners
                var topLeft = dotProductWithGridDirection(
                    gradientArray, xLower, yLower, xInGradientArraySpace, yInGradientArraySpace);
                var topRight = dotProductWithGridDirection(
                    gradientArray, xUpper, yLower, xInGradientArraySpace, yInGradientArraySpace);
                var bottomLeft = dotProductWithGridDirection(
                    gradientArray, xLower, yUpper, xInGradientArraySpace, yInGradientArraySpace);
                var bottomRight = dotProductWithGridDirection(
                    gradientArray, xUpper, yUpper, xInGradientArraySpace, yInGradientArraySpace);

                // Interpolation weights
                var tx = Interpolate.SmoothStep(0f, 1f, (float) x - xLower);
                var ty = Interpolate.SmoothStep(0f, 1f, (float) y - yLower);

                return Interpolate.BiLerp(topLeft, topRight, bottomLeft, bottomRight, tx, ty);
            }

            private static float dotProductWithGridDirection(
                Vector2[,] gradientArray, int gridX, int gridY, float x, float y)
            {
                var distance = new Vector2(x, y) - new Vector2(gridX, gridY);
                return Vector2.Dot(distance.Normalized(), gradientArray[gridX, gridY]);
            }

            private void resetTilemap(TileGeometry.TileType tileType)
            {
                foreach (var t in tilemap)
                {
                    tilemap[t] = tileType;
                }
            }

            private void createPathsFromNoiseTilemap(Tilemap<double> perlinTilemap)
            {
                logger.Trace?.Log("Digging paths to all corners");
                var result = doAllPairPathFindingFromTile(perlinTilemap, Tile.Origin);

                var corners = Directions
                    .All
                    .Enumerate()
                    .Select(dir => Tile.Origin.Offset(dir.Step() * tilemap.Radius));
                foreach (var start in corners)
                {
                    var curr = start;
                    while (curr != Tile.Origin)
                    {
                        tilemap[curr] = TileGeometry.TileType.Floor;
                        curr = result[curr].Parent;
                    }
                }
            }

            private Tilemap<(Tile Parent, double Cost)> doAllPairPathFindingFromTile(
                Tilemap<double> perlinTilemap, Tile origin)
            {
                var q = new PriorityQueue<double, Tile>();
                var result = new Tilemap<(Tile Parent, double Cost)>(perlinTilemap.Radius);

                result.ForEach(t => result[t] = (origin, double.PositiveInfinity));

                q.Enqueue(0, origin);
                result[origin] = (origin, 0);

                while (q.Count > 0)
                {
                    var (currPriority, currTile) = q.Dequeue();

                    foreach (var neighbor in level.ValidNeighboursOf(currTile))
                    {
                        var costToNeighbor = result[neighbor].Cost;
                        var candidateCost = currPriority + perlinTilemap[neighbor];

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

                foreach (var tile in tilemap.SpiralCenteredAt(Tile.Origin, radius))
                {
                    tilemap[tile] = TileGeometry.TileType.Floor;
                }
            }

            private void carve(Tilemap<double> perlinTilemap)
            {
                var q = new Queue<Tile>(tilemap.Where(isType(TileGeometry.TileType.Floor)));

                while (q.Count > 0)
                {
                    var curr = q.Dequeue();
                    foreach (var neighbor in level.ValidNeighboursOf(curr).Where(isType(TileGeometry.TileType.Wall)))
                    {
                        if (random.NormalDouble(0, .3) >= perlinTilemap[curr])
                        {
                            var type = random.NextDouble() < .2 ? TileGeometry.TileType.Crevice : tilemap[curr];
                            tilemap[neighbor] = type;
                            q.Enqueue(neighbor);
                        }
                    }
                }

                Func<Tile, bool> isType(TileGeometry.TileType type) => tile => tilemap[tile] == type;
            }
        }
    }
}
