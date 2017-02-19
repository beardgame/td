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
using Tile = Bearded.TD.Game.Tiles.Tile<Bearded.TD.Game.World.TileInfo>;
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

        public void Fill(Tilemap<TileInfo> tilemap)
        {
            logger.Debug.Log($"Started generating map with radius {tilemap.Radius}");
            var timer = Stopwatch.StartNew();
            var gen = new Generator(tilemap, logger);
            logger.Trace.Log("Filling tilemap");
            gen.FillAll(0.95);
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
            gen.ConnectRandomIntersections();
            gen.ClearTunnels();
            logger.Debug.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");
        }

        private class Generator
        {
            private readonly Tilemap<TileInfo> tilemap;
            private readonly Logger logger;
            private readonly Level level;
            private List<Position2> intersections = new List<Position2>();
            private List<Tuple<Position2, Position2>> tunnels = new List<Tuple<Position2, Position2>>();

            public Generator(Tilemap<TileInfo> tilemap, Logger logger)
            {
                this.tilemap = tilemap;
                this.logger = logger;
                level = new Level(tilemap);
            }

            public void ClearTunnels() => tunnels.ForEach(clearTunnel);

            private void clearTunnel(Tuple<Position2, Position2> tunnel)
                => clearTunnel(level.GetTile(tunnel.Item1), level.GetTile(tunnel.Item2));

            private void clearTunnel(Tile start, Tile goal)
            {
                var goalPosition = level.GetPosition(goal);
                var tile = start;
                open(tile);
                while (tile != goal)
                {
                    var vectorToGoal = goalPosition - level.GetPosition(tile);
                    var directionToGoal = vectorToGoal.Direction;

                    var directionToStep = directionToGoal.Hexagonal();
                    tile = tile.Neighbour(directionToStep);

                    open(tile);
                }
            }

            public void GenerateRandomIntersections()
            {
                intersections.Add(level.GetPosition(center));
                intersections.AddRange(corners.Select(level.GetPosition));

                generateIntersectionsOnCircle(5, tilemap.Radius * 0.5f);
            }

            private void generateIntersectionsOnCircle(int count, float radius)
            {
                var angleBetweenIntersections = 360.Degrees() / count;
                var levelRadius = radius * Constants.Game.World.HexagonWidth.U();

                var startAngle = Direction2.FromDegrees(StaticRandom.Float(360));

                for (int i = 0; i < count; i++)
                {
                    var angle = startAngle + angleBetweenIntersections * i;
                    var point = new Position2() + angle.Vector * levelRadius;
                    intersections.Add(point);
                }
            }

            public void ConnectRandomIntersections()
            {
                var graph = new List<Position2>();

                var verticesToAdd = intersections;

                graph.Add(verticesToAdd.First());

                foreach (var vertex in verticesToAdd.Skip(1))
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
                    tunnels.Add(Tuple.Create(vertex, connectTo.Value));
                }
            }

            private bool isValidArc(Position2 v1, Position2 v2)
            {
                var line = Line.Between(v1.NumericValue, v2.NumericValue);
                return tunnels.All(a => !Line.Between(a.Item1.NumericValue, a.Item2.NumericValue)
                    .IntersectsAsSegments(line));
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
                => tile.Info.SetTileType(type);

            private IEnumerable<Tile> spiral(Tile tile, int radius)
                => tilemap.SpiralCenteredAt(tile, radius);

            private Tile center => tile(0, 0);
            private Tile tile(int x, int y) => new Tile(tilemap, x, y);
        }
    }
}