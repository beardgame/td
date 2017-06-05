using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using Tile = Bearded.TD.Game.Tiles.Tile<Bearded.TD.Game.World.TileInfo.Type>;
using static Bearded.TD.Game.World.TileInfo.Type;
using static Bearded.TD.Game.Tiles.Directions;

namespace Bearded.TD.Game.Generation
{
    class DefaultTilemapGenerator : ITilemapGenerator
    {
        private readonly Logger logger;

        public DefaultTilemapGenerator(Logger logger)
        {
            this.logger = logger;
        }

        public Tilemap<TileInfo.Type> Generate(int radius)
        {
            logger.Debug.Log($"Started generating map with radius {radius}");
            var timer = Stopwatch.StartNew();
            var tilemap = new Tilemap<TileInfo.Type>(radius);
            var gen = new Generator(tilemap, logger);
            logger.Trace.Log("Filling tilemap");
            gen.FillAll();
            gen.ClearCenter(tilemap.Radius - 1, 0.1);
            logger.Trace.Log("Clearing tilemap center and corners");
            gen.ClearCenter(2);
            gen.ClearCenter(3, 0.5);
            gen.ClearCenter(5, 0.2);
            gen.ClearCorners(1);
            gen.ClearCorners(2, 0.5);
            gen.ClearCorners(3, 0.2);
            logger.Trace.Log("Generating random intersections");
            gen.GenerateRandomIntersections();
            logger.Trace.Log("Connecting random intersections");
            gen.ConnectAllIntersections();
            gen.ConnectRandomIntersections(0.2);
            gen.ConnectCornersToGraph();
            logger.Trace.Log("Digging tunnels");
            gen.ClearTunnels();

            logger.Debug.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return tilemap;
        }

        private class Generator
        {
            private readonly Tilemap<TileInfo.Type> tilemap;
            private readonly Logger logger;
            private readonly Level<TileInfo.Type> level;
            private List<Position2> intersections = new List<Position2>();
            private List<Tuple<Position2, Position2>> tunnels = new List<Tuple<Position2, Position2>>();

            public Generator(Tilemap<TileInfo.Type> tilemap, Logger logger)
            {
                this.tilemap = tilemap;
                this.logger = logger;
                level = new Level<TileInfo.Type>(tilemap);
            }

            public void ClearTunnels() => tunnels.ForEach(clearTunnel);

            private void clearTunnel(Tuple<Position2, Position2> tunnel)
                => clearTunnel(level.GetTile(tunnel.Item1), level.GetTile(tunnel.Item2));

            private void clearTunnel(Tile start, Tile goal)
            {
                var goalPosition = level.GetPosition(goal);
                var tile = start;
                var currentDirection = (goalPosition - level.GetPosition(tile)).Direction;
                open(tile);
                while (tile != goal)
                {
                    var vectorToGoal = goalPosition - level.GetPosition(tile);
                    var directionToGoal = vectorToGoal.Direction;

                    var distance = vectorToGoal.LengthSquared;
                    var variationFactor = (1 - 1 / (distance.NumericValue * 0.2f + 1));
                    var variation = variationFactor * 60;

                    currentDirection += StaticRandom.Float(-variation, variation).Degrees();
                    currentDirection = currentDirection.TurnedTowards(directionToGoal, 0.3f / variationFactor);

                    var directionToStep = currentDirection.Hexagonal();

                    var newTile = tile.Neighbour(directionToStep);
                    if (!newTile.IsValid)
                    {
                        newTile = tile.Neighbour(directionToGoal.Hexagonal());
                        currentDirection = directionToGoal;
                    }

                    tile = newTile;

                    open(tile);
                    spray(tile.Neighbours, open, 0.1);
                }
            }

            public void ConnectCornersToGraph()
            {
                corners.Select(level.GetPosition).ForEach(p => connectVertexToGraph(p, intersections));
            }

            public void GenerateRandomIntersections()
            {
                intersections.Add(level.GetPosition(center));

                generateIntersectionsOnCircle(2, tilemap.Radius * 0.2f);
                generateIntersectionsOnCircle(3, tilemap.Radius * 0.5f);
                generateIntersectionsOnCircle(7, tilemap.Radius * 0.8f);
            }

            private void generateIntersectionsOnCircle(int count, float radius)
            {
                var angleBetweenIntersections = 360.Degrees() / count;
                var levelRadius = radius * Constants.Game.World.HexagonWidth.U();

                var startAngle = Direction2.FromDegrees(StaticRandom.Float(360));

                var angleVariance = angleBetweenIntersections.Radians * 0.3f;

                for (var i = 0; i < count; i++)
                {
                    var angle = startAngle + angleBetweenIntersections * i
                                + StaticRandom.Float(-angleVariance, angleVariance).Radians();
                    var point = new Position2() + angle.Vector
                                * (levelRadius * StaticRandom.Float(0.8f, 1));
                    intersections.Add(point);
                }
            }

            public void ConnectRandomIntersections(double fraction)
            {
                foreach (var v1 in intersections)
                {
                    foreach (var v2 in intersections)
                    {
                        if (!StaticRandom.Bool(fraction))
                            continue;

                        if (isValidArc(v1, v2))
                            addTunnel(v1, v2);
                    }
                }
            }

            public void ConnectAllIntersections()
            {
                var graph = new List<Position2>();

                var verticesToAdd = intersections.Shuffled();

                graph.Add(verticesToAdd.First());

                foreach (var vertex in verticesToAdd.Skip(1))
                {
                    connectVertexToGraph(vertex, graph);
                    graph.Add(vertex);
                }
            }

            private void connectVertexToGraph(Position2 vertex, List<Position2> graph)
            {
                var verticesByDistance = graph.OrderBy(v => (v - vertex).LengthSquared);
                var connectTo = verticesByDistance
                    .Select(v => isValidArc(vertex, v) ? (Position2?) v : null)
                    .FirstOrDefault(v => v.HasValue);
                if (connectTo == null)
                {
                    logger.Debug.Log("Could not find possible non-intersecting arc. Selecting closest.");
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

            private void spray(IEnumerable<Tile> area, Action<Tile> action, double fraction)
                => area.ForEach(t =>
                {
                    if (fraction >= 1 || StaticRandom.Bool(fraction)) action(t);
                });

            private IEnumerable<Tile> corners => Tilemap.Directions.Select(corner);

            private Tile corner(Direction direction)
                => center.Offset(direction.Step() * tilemap.Radius);

            private void open(Tile tile) => set(tile, Floor);
            private void close(Tile tile) => set(tile, Wall);

            private void set(Tile tile, TileInfo.Type type)
                => tilemap[tile] = type;

            private IEnumerable<Tile> spiral(Tile tile, int radius)
                => tilemap.SpiralCenteredAt(tile, radius);

            private Tile center => tile(0, 0);
            private Tile tile(int x, int y) => new Tile(tilemap, x, y);
        }
    }
}
