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

        public PerlinTilemapGenerator(Logger logger, int gridSize = 5)
        {
            this.logger = logger;
            this.gridSize = gridSize;
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var typeTilemap = new Tilemap<TileType>(radius);
            var hardnessTilemap = new Tilemap<double>(radius);
            var gen = new Generator(typeTilemap, hardnessTilemap, seed, logger, gridSize);

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
            private readonly Level level;
            private readonly Random random;
            private readonly Logger logger;
            private readonly int gridSize;

            public Generator(
                Tilemap<TileType> typeTilemap,
                Tilemap<double> hardnessTilemap,
                int seed,
                Logger logger,
                int gridSize)
            {
                this.typeTilemap = typeTilemap;
                this.hardnessTilemap = hardnessTilemap;
                level = new Level(typeTilemap.Radius);
                random = new Random(seed);
                this.logger = logger;
                this.gridSize = gridSize;
            }

            public void GenerateTilemap()
            {
                var tileArrayDimension = 2 * typeTilemap.Radius + 1;

                var gradientArrayDimension = Mathf.CeilToInt((double) tileArrayDimension / gridSize) + 1;
                var gradientArray = createRandomGradientGrid(gradientArrayDimension);

                fillHardnessTilemapWithNormalizedPerlin(gradientArray);

                resetTilemap(TileType.Wall);

                createPathsFromNoiseTilemap();
                clearCenter(4);

                carve();
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
            private void fillHardnessTilemapWithNormalizedPerlin(Vector2[,] gradientArray)
            {
                foreach (var tile in typeTilemap)
                {
                    var perlin = perlinAt(
                        gradientArray, tile.X + hardnessTilemap.Radius, tile.Y + hardnessTilemap.Radius);
                    var normalizedPerlin = (perlin * .5 + .5).Clamped(0, 1);
                    hardnessTilemap[tile] = normalizedPerlin;
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
                    gradientArray, xLower, yUpper, xInGradientArraySpace, yInGradientArraySpace);
                var topRight = dotProductWithGridDirection(
                    gradientArray, xUpper, yUpper, xInGradientArraySpace, yInGradientArraySpace);
                var bottomLeft = dotProductWithGridDirection(
                    gradientArray, xLower, yLower, xInGradientArraySpace, yInGradientArraySpace);
                var bottomRight = dotProductWithGridDirection(
                    gradientArray, xUpper, yLower, xInGradientArraySpace, yInGradientArraySpace);

                // Interpolation weights
                var tx = Interpolate.SmoothStep(0f, 1f, xInGradientArraySpace - xLower);
                var ty = 1 - Interpolate.SmoothStep(0f, 1f, yInGradientArraySpace - yLower);

                var top = Interpolate.Lerp(topLeft, topRight, tx);
                var bottom = Interpolate.Lerp(bottomLeft, bottomRight, tx);
                return Interpolate.Lerp(top, bottom, ty);

//                return Interpolate.BiLerp(topLeft, topRight, bottomLeft, bottomRight, tx, ty);
            }

            private static float dotProductWithGridDirection(
                Vector2[,] gradientArray, int gridX, int gridY, float x, float y)
            {
                var distance = new Vector2(x, y) - new Vector2(gridX, gridY);
                return Vector2.Dot(distance, gradientArray[gridX, gridY]);
            }

            private void resetTilemap(TileType tileType)
            {
                foreach (var t in typeTilemap)
                {
                    typeTilemap[t] = tileType;
                }
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
                var q = new Queue<Tile>(typeTilemap.Where(isType(TileType.Floor)));

                while (q.Count > 0)
                {
                    var curr = q.Dequeue();
                    foreach (var neighbor in level.ValidNeighboursOf(curr).Where(isType(TileType.Wall)))
                    {
                        if (random.NormalDouble(0, .3) >= hardnessTilemap[curr])
                        {
                            var type = random.NextDouble() < .2 ? TileType.Crevice : typeTilemap[curr];
                            typeTilemap[neighbor] = type;
                            q.Enqueue(neighbor);
                        }
                    }
                }

                Func<Tile, bool> isType(TileType type) => tile => typeTilemap[tile] == type;
            }
        }
    }
}
