using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation
{
    sealed class GraphGenerator
    {
        private readonly Random random;

        public GraphGenerator(Random random)
        {
            this.random = random;
        }

        public (List<Tile> Vertices, IEnumerable<(Tile, Tile)> Edges) GenerateGraph(int tilemapRadius)
        {
            // Generates a connected graph with points on concentric rings around the origin. The graph:
            // * is guaranteed to be connected;
            // * minimises the number of intersecting edges.

            var vertices = GenerateVerticesAlongConcentricRings(tilemapRadius).ToList();
            var edges = GenerateEdgesForConnectedGraph(vertices);

            return (vertices, edges);
        }

        public IEnumerable<Tile> GenerateVerticesAlongConcentricRings(int tilemapRadius)
        {
            var level = new Level(tilemapRadius);

            var rings = generateRings(tilemapRadius);
            return generateTilesOnRings(level, rings);
        }

        public IEnumerable<(Tile, Tile)> GenerateEdgesForConnectedGraph(List<Tile> vertices)
        {
            var treeEdges = createSpanningTree(vertices);
            return expandEdgesWithRandomEdges(vertices, treeEdges, .2);
        }

        private static IEnumerable<(float Radius, int NumPoints)> generateRings(int tilemapRadius)
        {
            // This value has been determined by experimentation as something that looks good.
            var numRings = (int)(Math.Sqrt(tilemapRadius) / 1.4);

            // We put the points in the inner 90% of the tilemap.
            // We also add an artificial ring at the outside to prevent having points too close to the edge.
            var radiusStep = .9f * tilemapRadius / (numRings + 1);

            return Enumerable.Range(0, numRings).Select(i => (Radius: radiusStep * (i + 1), NumPoints: i + 2));
        }

        private IEnumerable<Tile> generateTilesOnRings(Level level, IEnumerable<(float Radius, int NumPoints)> rings)
        {
            return rings.SelectMany(ring => generateTilesOnRing(level, ring.Radius, ring.NumPoints));
        }

        private IEnumerable<Tile> generateTilesOnRing(Level level, float radius, int numPoints)
        {
            var angleBetweenIntersections = 360.Degrees() / numPoints;
            var levelRadius = radius * Constants.Game.World.HexagonWidth.U();

            var startAngle = Direction2.FromDegrees(random.NextFloat(360));

            // Allow a 30% angle offset in either direction along the ring.
            var angleVariance = angleBetweenIntersections.Degrees * 0.3f;

            for (var i = 0; i < numPoints; i++)
            {
                var angle = startAngle + angleBetweenIntersections * i
                    + random.NextFloat(-angleVariance, angleVariance).Degrees();
                // Vary the distance to the point by moving it towards the origin by up to 20%.
                var point = Position2.Zero + angle.Vector * (levelRadius * random.NextFloat(0.8f, 1));

                var tile = Level.GetTile(point);
                if (!level.IsValid(tile))
                    continue;

                yield return tile;
            }
        }

        private IEnumerable<(Tile, Tile)> createSpanningTree(IEnumerable<Tile> tiles)
        {
            // Greedily builds a spanner tree by linking a tile to its closest neighbour.

            var edges = new List<(Tile From, Tile To)>();

            var addedTiles = new List<Tile>();
            var tilesToAdd = tiles.Shuffled(random);
            addedTiles.Add(tilesToAdd.First());

            foreach (var tile in tilesToAdd.Skip(1))
            {
                var tilePos = Level.GetPosition(tile);
                var addedTilesByDistance =
                    addedTiles.OrderBy(t => (Level.GetPosition(t) - tilePos).LengthSquared).ToList();
                var closestWithoutIntersection = addedTilesByDistance.FirstOrDefault(t => hasNoIntersection(t, tile));

                // Just pick the closest if there is no tile that leads to no intersection.
                if (closestWithoutIntersection == default(Tile))
                {
                    closestWithoutIntersection = addedTilesByDistance.First();
                }

                edges.Add((tile, closestWithoutIntersection));
            }

            return edges;

            bool hasNoIntersection(Tile from, Tile to)
            {
                var segment = lineSegmentBetween(from, to);
                return !edges.Any(edge => lineSegmentBetween(edge.From, edge.To).IntersectsAsSegments(segment));
            }
        }

        private IEnumerable<(Tile, Tile)> expandEdgesWithRandomEdges(
            List<Tile> vertices, IEnumerable<(Tile From, Tile To)> existingEdges, double probabilityOfConnecting)
        {
            // Returns all existing edges, including an approximate fraction of other edges.

            var edgeList = existingEdges.ToList();

            foreach (var edge in edgeList)
            {
                yield return edge;
            }

            foreach (var v1 in vertices)
            {
                foreach (var v2 in vertices)
                {
                    if (random.NextBool(probabilityOfConnecting) && isValidEdge(v1, v2))
                    {
                        yield return (v1, v2);
                    }
                }
            }

            bool isValidEdge(Tile v1, Tile v2)
            {
                if (v1 == v2) return false;

                var segment = lineSegmentBetween(v1, v2);

                foreach (var (from, to) in edgeList)
                {
                    if (from == v1 && to == v2) return false;
                    if (from == v2 && to == v1) return false;
                    if (lineSegmentBetween(from, to).IntersectsAsSegments(segment)) return false;
                }

                return true;
            }
        }

        private static Line lineSegmentBetween(Tile from, Tile to)
            => Line.Between(Level.GetPosition(from).NumericValue, Level.GetPosition(to).NumericValue);
    }
}
