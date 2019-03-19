using System;
using System.Diagnostics;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.IO;
using OpenTK;

namespace Bearded.TD.Game.Generation
{
    sealed class PerlinTilemapGenerator : ITilemapGenerator
    {
        private readonly Logger logger;
        private readonly int gridSize;
        private readonly double fillThreshold;
        private readonly double creviceThreshold;

        public PerlinTilemapGenerator(
            Logger logger, int gridSize = 3, double fillThreshold = .7, double creviceThreshold = .2)
        {
            this.logger = logger;
            this.gridSize = gridSize;
            this.fillThreshold = fillThreshold;
            this.creviceThreshold = creviceThreshold;
        }

        public Tilemap<TileGeometry.TileType> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var tilemap = new Tilemap<TileGeometry.TileType>(radius);
            var gen = new Generator(tilemap, seed, logger, gridSize, fillThreshold, creviceThreshold);

            gen.GenerateTilemap();

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return tilemap;
        }

        private class Generator
        {
            private readonly Tilemap<TileGeometry.TileType> tilemap;
            private readonly Random random;
            private readonly Logger logger;
            private readonly int gridSize;
            private readonly double fillThreshold;
            private readonly double creviceThreshold;

            public Generator(
                Tilemap<TileGeometry.TileType> tilemap,
                int seed,
                Logger logger,
                int gridSize,
                double fillThreshold,
                double creviceThreshold)
            {
                this.tilemap = tilemap;
                random = new Random(seed);
                this.logger = logger;
                this.gridSize = gridSize;
                this.fillThreshold = fillThreshold;
                this.creviceThreshold = creviceThreshold;
            }

            public void GenerateTilemap()
            {
                var tileArrayDimension = 2 * tilemap.Radius + 1;

                var gradientArrayDimension = Mathf.CeilToInt((double) tileArrayDimension / gridSize) + 1;
                var gradientArray = createRandomGradientGrid(gradientArrayDimension);

                var perlinTilemap = new Tilemap<double>(tilemap.Radius);
                fillTilemapWithNormalizedPerlin(gradientArray, perlinTilemap);

                populateTileTypeFromNoiseTilemap(perlinTilemap);

                clearCenter(4);
            }

            private void clearCenter(int radius)
            {
                logger.Trace?.Log("Clearing center tiles");

                foreach (var tile in tilemap.SpiralCenteredAt(Tile.Origin, radius))
                {
                    tilemap[tile] = TileGeometry.TileType.Floor;
                }
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

            private void populateTileTypeFromNoiseTilemap(Tilemap<double> perlinTilemap)
            {
                foreach (var tile in tilemap)
                {
                    var normalizedPerlin = perlinTilemap[tile];
                    if (normalizedPerlin < creviceThreshold)
                    {
                        tilemap[tile] = TileGeometry.TileType.Crevice;
                    }
                    else if (normalizedPerlin > fillThreshold)
                    {
                        tilemap[tile] = TileGeometry.TileType.Wall;
                    }
                    else
                    {
                        tilemap[tile] = TileGeometry.TileType.Floor;
                    }
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
                return Vector2.Dot(distance, gradientArray[gridX, gridY]);
            }
        }
    }
}
