using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Generation
{
    sealed class PerlinTilemapGenerator : ITilemapGenerator
    {
        private const int hardnessRampDistance = 5;

        private readonly Logger logger;
        private readonly LevelDebugMetadata debugMetadata;

        public PerlinTilemapGenerator(Logger logger, LevelDebugMetadata debugMetadata)
        {
            this.logger = logger;
            this.debugMetadata = debugMetadata;
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var typeTilemap = new Tilemap<TileType>(radius, _ => TileType.Wall);
            var hardnessTilemap = new Tilemap<double>(radius);
            var gen = new Generator(typeTilemap, hardnessTilemap, seed, logger, debugMetadata);

            gen.GenerateTilemap();

            logger.Trace?.Log("Copying tilemap data into final tilemap.");

            var tilemap = new Tilemap<TileGeometry>(radius);
            foreach (var t in tilemap)
            {
                tilemap[t] = new TileGeometry(typeTilemap[t], hardnessTilemap[t], Unit.Zero);
            }

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return tilemap;
        }

        private class Generator
        {
            private readonly Tilemap<TileType> typeTilemap;
            private readonly Tilemap<double> hardnessTilemap;
            private readonly Logger logger;
            private readonly LevelDebugMetadata levelDebugMetadata;
            private readonly Level level;
            private readonly Random random;
            private readonly PerlinSourcemapGenerator perlinSourcemapGenerator;
            private readonly GraphGenerator graphGenerator;

            public Generator(
                Tilemap<TileType> typeTilemap,
                Tilemap<double> hardnessTilemap,
                int seed,
                Logger logger,
                LevelDebugMetadata levelDebugMetadata)
            {
                this.typeTilemap = typeTilemap;
                this.hardnessTilemap = hardnessTilemap;
                this.logger = logger;
                this.levelDebugMetadata = levelDebugMetadata;
                level = new Level(typeTilemap.Radius);
                random = new Random(seed);
                perlinSourcemapGenerator = new PerlinSourcemapGenerator(random);
                graphGenerator = new GraphGenerator(random);
            }

            public void GenerateTilemap()
            {
                var sourceMap1 = new Tilemap<double>(hardnessTilemap.Radius);
                var sourceMap2 = new Tilemap<double>(hardnessTilemap.Radius);
                var sourceMap3 = new Tilemap<double>(hardnessTilemap.Radius);

                // Generate three different perlin noise maps with different grid sizes (= 1 / frequency). We rotate
                // each of them by 60% relative to each other to hide the
                // We use the absolute value which causes us to have sharp valleys in the noise map. This creates a
                // better texture for digging tunnels in. We also multiply by some factors to amplify low frequency
                // noise.
                perlinSourcemapGenerator.FillTilemapWithPerlinNoise(
                    sourceMap1, 10, (tilemap, tile) => 1.8 * Math.Abs(tilemap[tile.RotatedClockwiseAroundOrigin()]));
                perlinSourcemapGenerator.FillTilemapWithPerlinNoise(
                    sourceMap2, 5, (tilemap, tile) => 0.9 * Math.Abs(tilemap[tile.RotatedCounterClockwiseAroundOrigin()]));
                // Keep the lowest amplitude noise coordinate-system aligned
                perlinSourcemapGenerator.FillTilemapWithPerlinNoise(
                    sourceMap3, 3, (tilemap, tile) => 0.3 * Math.Abs(tilemap[tile]));

                foreach (var tile in hardnessTilemap)
                {
                    double hardnessOverride = 0;
                    var distanceFromEdge = hardnessTilemap.Radius - tile.Radius;

                    // Make the hardness artificially high near the map edges to direct paths to the interior.
                    if (distanceFromEdge < hardnessRampDistance)
                    {
                        hardnessOverride = 1 - (double) distanceFromEdge / hardnessRampDistance;
                    }

                    var desiredHardness = (sourceMap1[tile] + sourceMap2[tile] + sourceMap3[tile]).Clamped(0, 1);
                    hardnessTilemap[tile] = Math.Max(desiredHardness, hardnessOverride);
                }

                //createPathsToCorners();
                createTunnels();
                clearCenter(4);
                clearCorners(2);

                carve();

                createCrevices();
            }

            private void createPathsToCorners()
            {
                logger.Trace?.Log("Digging paths to all corners");

                var result = createPathFindingTilemapToTile(level.Center, level.Corners);

                foreach (var start in level.Corners)
                {
                    digAlongShortestPath(start, level.Center, result);
                }
            }

            private void createTunnels()
            {
                // Start by creating a V-shape around the base of impassable segments that tunnels won't cross.
                // This will heavily bias maps generated with only directions of the base open to one or two directions.
                const int impassableSegmentRadius = 6;
                var hardestTile =
                    Tilemap.GetRingCenteredAt(level.Center, impassableSegmentRadius).MaxBy(t => hardnessTilemap[t]);
                var thirdClockwise = hardestTile
                    .RotatedClockwiseAroundOrigin()
                    .RotatedClockwiseAroundOrigin();
                var thirdCounterClockwise = hardestTile
                    .RotatedCounterClockwiseAroundOrigin()
                    .RotatedCounterClockwiseAroundOrigin();
                var impassableSegments = new List<(Tile, Tile)>()
                {
                    (hardestTile, thirdClockwise),
                    (hardestTile, thirdCounterClockwise)
                };
#if DEBUG
                foreach (var (from, to) in impassableSegments)
                {
                    levelDebugMetadata.AddSegment(Level.GetPosition(from), Level.GetPosition(to), Color.Red);
                }
#endif

                var paths = graphGenerator.GenerateEdgesForConnectedGraph(
                    getTilesForTunnelGraph().ToList(), impassableSegments);

                foreach (var (from, to) in paths)
                {
#if DEBUG
                    levelDebugMetadata.AddSegment(Level.GetPosition(from), Level.GetPosition(to), Color.Aqua);
#endif
                    var pathFindingResult = createPathFindingTilemapToTile(to, ImmutableList.Create(from));
                    digAlongShortestPath(from, to, pathFindingResult);
                }
            }

            private IEnumerable<Tile> getTilesForTunnelGraph()
            {
                var points = graphGenerator.GenerateVerticesAlongConcentricRings(typeTilemap.Radius);

                // Use this if you already have some tunnels and want avoid making other points next to it.
//                var snappedPoints = points.Select(t => snapTileToAlreadyCarved(t, 3)).ToList();

                // Use this if you want to make it less likely that points show up in hard rock.
                var pushedPoints = points.Select(t => pushTileToLowestHardnessInRange(t, 5)).Distinct();

                return pushedPoints.Concat(level.Corners).Concat(ImmutableList.Create(level.Center));
            }

            private Tile snapTileToAlreadyCarved(Tile tile, int searchRadius)
            {
                var floorTilesInRange = Tilemap.GetSpiralCenteredAt(tile, searchRadius)
                    .Where(t => level.IsValid(t) && typeTilemap[t] == TileType.Floor)
                    .ToList();
                return floorTilesInRange.Count > 0 ? floorTilesInRange[random.Next(floorTilesInRange.Count)] : tile;
            }

            private Tile pushTileToLowestHardnessInRange(Tile tile, int searchRadius)
            {
                return Tilemap.GetSpiralCenteredAt(tile, searchRadius)
                    .Where(level.IsValid)
                    .MinBy(t => hardnessTilemap[t]);
            }

            private Tilemap<(Tile Parent, double Cost)> createPathFindingTilemapToTile(
                Tile origin, IEnumerable<Tile> targets)
            {
                var targetsLeft = new HashSet<Tile>(targets);

                var q = new PriorityQueue<double, Tile>();
                var result = new Tilemap<(Tile Parent, double Cost)>(hardnessTilemap.Radius);

                result.ForEach(t => result[t] = (origin, double.PositiveInfinity));

                q.Enqueue(0, origin);
                result[origin] = (origin, 0);

                while (q.Count > 0 && targetsLeft.Count > 0)
                {
                    var (currPriority, currTile) = q.Dequeue();
                    targetsLeft.Remove(currTile);

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

            private void digAlongShortestPath(Tile source, Tile target, Tilemap<(Tile Parent, double Cost)> paths)
            {
                var curr = source;
                while (curr != target)
                {
                    typeTilemap[curr] = TileType.Floor;
                    var parent = paths[curr].Parent;

#if DEBUG
                    levelDebugMetadata.AddSegment(Level.GetPosition(curr), Level.GetPosition(parent), Color.Lime);
#endif

                    curr = parent;
                }
                typeTilemap[target] = TileType.Floor;
            }

            private void clearCenter(int radius)
            {
                logger.Trace?.Log("Clearing center tiles");

                clearAroundTile(level.Center, radius);
            }

            private void clearCorners(int radius)
            {
                logger.Trace?.Log("Clearing corner tiles");

                foreach (var corner in level.Corners) clearAroundTile(corner, radius);
            }

            private void clearAroundTile(Tile t, int radius)
            {
                foreach (var tile in typeTilemap.SpiralCenteredAt(t, radius).Where(level.IsValid))
                {
                    typeTilemap[tile] = TileType.Floor;
                }
            }

            private void carve()
            {
                // The value here has been chosen based on experimentation on what looks good.
                carveEverythingBelowHardness(0.1);

                var q = new Queue<Tile>(typeTilemap.Where(isType(TileType.Floor)));

                while (q.Count > 0)
                {
                    var curr = q.Dequeue();
                    foreach (var neighbor in level.ValidNeighboursOf(curr).Where(isType(TileType.Wall)))
                    {
                        // The standard deviation here has been chosen based on experimentation on what looks good.
                        // We use the hardness cubed to increase the contrast between low and high hardness, causing a
                        // wider variance in tunnel width.
                        if (random.NormalDouble(0, 0.05) < hardnessTilemap[curr].Cubed()) continue;

                        var type = typeTilemap[curr];
                        typeTilemap[neighbor] = type;
                        q.Enqueue(neighbor);
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

            private void createCrevices()
            {
                for (var i = 0; i < numCrevices; i++)
                {
                    tryCreateCrevice();
                }
            }

            private void tryCreateCrevice()
            {
                var tile = level.RandomTile(random);
                if (typeTilemap[tile] != TileType.Wall) return;

                var creviceTiles = new HashSet<Tile> { tile };
                var targetCreviceSize = random.Next(minTargetCreviceSize, maxTargetCreviceSize);

                while (creviceTiles.Count < targetCreviceSize)
                {
                    var closedNeighbours = level
                        .ValidNeighboursOf(tile)
                        .Where(t => typeTilemap[t] == TileType.Wall && !creviceTiles.Contains(t))
                        .ToList();
                    if (closedNeighbours.Count == 0) break;

                    tile = closedNeighbours.RandomElement(random);
                    creviceTiles.Add(tile);
                }

                if (creviceTiles.Count < minCreviceSize) return;

                foreach (var t in creviceTiles)
                {
                    typeTilemap[t] = TileType.Crevice;
                }
            }

            private int numCrevices => typeTilemap.Radius * typeTilemap.Radius / 15;
            private const int minCreviceSize = 3;
            private const int minTargetCreviceSize = 8;
            private int maxTargetCreviceSize => (int) Math.Sqrt(typeTilemap.Radius) * 3;
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

                var gradientArrayDimension = Mathf.CeilToInt(2 * maxDeviationFromCenter / gridSize) + 2;
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
