using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.World.TileType;
using static Bearded.TD.Tiles.Directions;

namespace Bearded.TD.Game.Generation
{
    class DefaultTilemapGenerator : ITilemapGenerator
    {
        private readonly Logger logger;
        private readonly LevelDebugMetadata levelDebugMetadata;

        public DefaultTilemapGenerator(Logger logger, LevelDebugMetadata levelDebugMetadata)
        {
            this.logger = logger;
            this.levelDebugMetadata = levelDebugMetadata;
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();
            var typeTilemap = new Tilemap<TileType>(radius);
            var gen = new Generator(typeTilemap, seed, logger, levelDebugMetadata);
            logger.Trace?.Log("Filling tilemap");
            gen.FillAll();
            gen.ClearCenter(typeTilemap.Radius - 1, 0.1);
            logger.Trace?.Log("Clearing tilemap center and corners");
            gen.ClearCenter(4);
            gen.ClearCenter(5, 0.3);
            gen.ClearCenter(7, 0.1);
            gen.ClearCorners(1);
            gen.ClearCorners(2, 0.5);
            gen.ClearCorners(3, 0.2);
            logger.Trace?.Log("Generating random intersections");
            gen.GenerateRandomIntersections();
            logger.Trace?.Log("Connecting random intersections");
            gen.ConnectAllIntersections();
            gen.ConnectRandomIntersections(0.2);
            gen.ConnectCornersToGraph();
            logger.Trace?.Log("Digging tunnels");
            gen.ClearTunnels();
            gen.DigDeep(radius * radius / 15);

            logger.Trace?.Log("Copy data to final tilemap");
            var tilemap = new Tilemap<TileGeometry>(radius);
            foreach (var t in tilemap)
                tilemap[t] = new TileGeometry(typeTilemap[t], gen.RandomHardness(), Unit.Zero);

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return tilemap;
        }

        private class Generator
        {
            private readonly Tilemap<TileType> tilemap;
            private readonly Random random;
            private readonly Logger logger;
            private readonly LevelDebugMetadata levelDebugMetadata;
            private readonly Level level;
            private readonly List<Position2> intersections = new List<Position2>();
            private readonly List<Tuple<Position2, Position2>> tunnels = new List<Tuple<Position2, Position2>>();

            public Generator(Tilemap<TileType> tilemap, int seed, Logger logger, LevelDebugMetadata levelDebugMetadata)
            {
                this.tilemap = tilemap;
                random = new Random(seed);
                this.logger = logger;
                this.levelDebugMetadata = levelDebugMetadata;
                level = new Level(tilemap.Radius);
            }

            public void ClearTunnels() => tunnels.ForEach(clearTunnel);

            private void clearTunnel(Tuple<Position2, Position2> tunnel)
                => clearTunnel(Level.GetTile(tunnel.Item1), Level.GetTile(tunnel.Item2));

            private void clearTunnel(Tile start, Tile goal)
            {
#if DEBUG
                levelDebugMetadata.AddSegment(Level.GetPosition(start), Level.GetPosition(goal), Color.Aqua);
#endif

                var goalPosition = Level.GetPosition(goal);
                var tile = start;
                var currentDirection = (goalPosition - Level.GetPosition(tile)).Direction;
                open(tile);
                while (tile != goal)
                {
                    var vectorToGoal = goalPosition - Level.GetPosition(tile);
                    var directionToGoal = vectorToGoal.Direction;

                    var distance = vectorToGoal.LengthSquared;
                    var variationFactor = (1 - 1 / (distance.NumericValue * 0.2f + 1));
                    var variation = variationFactor * 60;

                    currentDirection += random.NextFloat(-variation, variation).Degrees();
                    currentDirection = currentDirection.TurnedTowards(directionToGoal, 0.3f / variationFactor);

                    var directionToStep = currentDirection.Hexagonal();

                    var newTile = tile.Neighbour(directionToStep);
                    if (!isValidTile(newTile))
                    {
                        newTile = tile.Neighbour(directionToGoal.Hexagonal());
                        currentDirection = directionToGoal;
                    }

                    tile = newTile;

                    open(tile);
                    spray(spiral(tile, 1), open, 0.05);
                    spray(spiral(tile, 2), open, 0.04);
                }
            }

            public void ConnectCornersToGraph()
            {
                corners.Select(Level.GetPosition).ForEach(p => connectVertexToGraph(p, intersections));
            }

            public void GenerateRandomIntersections()
            {
                intersections.Add(Level.GetPosition(center));

                var circles = (int)(Math.Sqrt(tilemap.Radius) / 1.4);
                var radiusStep = tilemap.Radius / (float) (circles + 1) * 0.9f;

                for (var circle = 0; circle < circles; circle++)
                {
                    var count = circle + 2;
                    var radius = radiusStep * (circle + 1);
                    generateIntersectionsOnCircle(count, radius);
                }
            }

            private void generateIntersectionsOnCircle(int count, float radius)
            {
                var angleBetweenIntersections = 360.Degrees() / count;
                var levelRadius = radius * Constants.Game.World.HexagonWidth.U();

                var startAngle = Direction2.FromDegrees(random.NextFloat(360));

                var angleVariance = angleBetweenIntersections.Radians * 0.3f;

                for (var i = 0; i < count; i++)
                {
                    var angle = startAngle + angleBetweenIntersections * i
                                + random.NextFloat(-angleVariance, angleVariance).Radians();
                    var point = new Position2() + angle.Vector
                                * (levelRadius * random.NextFloat(0.8f, 1));

                    var tile = Level.GetTile(point);
                    if (!isValidTile(tile))
                        continue;

                    intersections.Add(point);
                }
            }

            public void ConnectRandomIntersections(double fraction)
            {
                foreach (var v1 in intersections)
                {
                    foreach (var v2 in intersections)
                    {
                        if (!random.NextBool(fraction))
                            continue;

                        if (isValidArc(v1, v2))
                            addTunnel(v1, v2);
                    }
                }
            }

            public void ConnectAllIntersections()
            {
                var graph = new List<Position2>();

                var verticesToAdd = intersections.Shuffled(random);

                graph.Add(verticesToAdd.First());

                foreach (var vertex in verticesToAdd.Skip(1))
                {
                    connectVertexToGraph(vertex, graph);
                    graph.Add(vertex);
                }
            }

            private void connectVertexToGraph(Position2 vertex, IEnumerable<Position2> graph)
            {
                var verticesByDistance = graph.OrderBy(v => (v - vertex).LengthSquared).ToList();
                var connectTo = verticesByDistance
                    .Select(v => isValidArc(vertex, v) ? (Position2?) v : null)
                    .FirstOrDefault(v => v.HasValue);
                if (connectTo == null)
                {
                    logger.Debug?.Log("Could not find possible non-intersecting arc. Selecting closest.");
                    connectTo = verticesByDistance.First();
                }
                addTunnel(vertex, connectTo.Value);
            }

            private void addTunnel(Position2 v1, Position2 v2) => tunnels.Add(Tuple.Create(v1, v2));

            private bool isValidArc(Position2 v1, Position2 v2)
            {
                var line = Line.Between(v1.NumericValue, v2.NumericValue);
                return tunnels.All(t =>
                    (t.Item1 != v2 || t.Item2 != v1) &&
                    !Line.Between(t.Item1.NumericValue, t.Item2.NumericValue)
                        .IntersectsAsSegments(line)
                );
            }

            public void FillAll(double fraction = 1)
                => spray(tilemap, close, fraction);

            public void ClearCenter(int radius, double fraction = 1)
                => spray(spiral(center, radius), open, fraction);

            public void ClearCorners(int radius, double fraction = 1)
                => ClearCorners(All, radius, fraction);

            public void ClearCorners(Directions directions, int radius, double fraction = 1)
                => directions.Enumerate().ForEach(d => ClearCorner(d, radius, fraction));

            public void ClearCorner(Direction direction, int radius, double fraction = 1)
                => spray(spiral(corner(direction), radius), open, fraction);

            public void DigDeep(int count = 10)
            {
                for (var i = 0; i < count; i++)
                {
                    var tile = randomTile();
                    if (tilemap[tile] != Wall)
                        continue;
                    var tiles = new List<Tile> {tile};

                    var maxCount = random.Next(8, (int) Math.Sqrt(tilemap.Radius) * 3);

                    while (tiles.Count < maxCount)
                    {
                        var closedNeighbours = level
                            .ValidNeighboursOf(tile)
                            .Where(t => tilemap[t] == Wall && !tiles.Contains(t))
                            .ToList();
                        if (closedNeighbours.Count == 0)
                            break;

                        tile = closedNeighbours.RandomElement(random);
                        tiles.Add(tile);
                    }

                    if (tiles.Count <= 3) continue;

                    foreach (var t in tiles)
                        set(t, Crevice);
                }
            }

            public double RandomHardness()
            {
                return random.NextDouble();
            }

            private Tile randomTile()
            {
                var r = tilemap.Radius;
                while (true)
                {
                    var tile = this.tile(random.Next(-r, r), random.Next(-r, r));

                    if (isValidTile(tile))
                        return tile;
                }
            }

            private bool isValidTile(Tile tile)
            {
                return tilemap.IsValidTile(tile);
            }

            private void spray(IEnumerable<Tile> area, Action<Tile> action, double fraction)
                => area.ForEach(t =>
                {
                    if (fraction >= 1 || random.NextBool(fraction)) action(t);
                });

            private IEnumerable<Tile> corners => Tilemap.Directions.Select(corner);

            private Tile corner(Direction direction)
                => center.Offset(direction.Step() * tilemap.Radius);

            private void open(Tile tile) => set(tile, Floor);
            private void close(Tile tile) => set(tile, Wall);

            private void set(Tile tile, TileType type)
                => tilemap[tile] = type;

            private IEnumerable<Tile> spiral(Tile tile, int radius)
                => tilemap.SpiralCenteredAt(tile, radius);

            private Tile center => tile(0, 0);
            private Tile tile(int x, int y) => new Tile(x, y);
        }
    }
}
