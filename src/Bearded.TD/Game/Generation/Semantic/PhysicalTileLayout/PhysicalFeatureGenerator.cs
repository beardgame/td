using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Logical;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

sealed class PhysicalFeatureGenerator
{
    private readonly Unit defaultNodeRadius;

    public PhysicalFeatureGenerator(Unit defaultNodeRadius)
    {
        this.defaultNodeRadius = defaultNodeRadius;
    }

    public List<PhysicalFeature> GenerateFeaturesWithAreasInInitialLocation(
        LogicalTilemap logicalTilemap, Random random)
    {
        var nodesByLogicalTile = createNodeFeatures(logicalTilemap, random);
        var connections = createNodeConnectionFeatures(logicalTilemap, nodesByLogicalTile, random);

        var crevices = createCreviceFeatures(logicalTilemap);

        return nodesByLogicalTile.Values
            .Concat<PhysicalFeature>(connections)
            .Concat(crevices)
            .ToList();
    }

    private Dictionary<Tile, PhysicalFeature.Node> createNodeFeatures(
        LogicalTilemap logicalTilemap, Random random)
    {
        var nodes = new Dictionary<Tile, PhysicalFeature.Node>();
        foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
        {
            var node = logicalTilemap[tile];
            if (node.Blueprint == null)
            {
                continue;
            }
            if (node.Biome == null)
            {
                throw new InvalidOperationException("All nodes should have a biome assigned");
            }

            var center = Position2.Zero + Level.GetPosition(tile).NumericValue * defaultNodeRadius * 2;

            // TODO: once we use multiple circles, passed from script files, rotate the feature
            // so that connections will line up more easily (circles know if they can connect to or not)
            // also make sure that all circles fit within the 'nodeRadius' circle
            // so that they won't overlap and intersect funny with other nodes for relaxation
            // (we can probably just scale the node feature graph by its max radius

            var nodeSizeRadius = node.Blueprint.Radius ?? defaultNodeRadius;

            var circle = new Circle(center, nodeSizeRadius * random.NextFloat(0.75f, 1.2f));

            var n = new PhysicalFeature.Node(node.Blueprint, node.Biome, ImmutableArray.Create(circle));

            nodes.Add(tile, n);
        }

        return nodes;
    }

    private List<PhysicalFeature.Connection> createNodeConnectionFeatures(
        LogicalTilemap logicalTilemap,
        Dictionary<Tile, PhysicalFeature.Node> nodes, Random random)
    {
        var connections = new List<PhysicalFeature.Connection>();
        foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
        {
            var node = logicalTilemap[tile];
            if (node.Blueprint == null)
                continue;

            tryDirection(tile, node, Direction.Right);
            tryDirection(tile, node, Direction.UpRight);
            tryDirection(tile, node, Direction.UpLeft);
        }

        return connections;

        void tryDirection(Tile tile, PlacedNode node, Direction dir)
        {
            if (!node.ConnectedTo.Includes(dir))
                return;

            var neighborTile = tile.Neighbor(dir);

            // TODO: look for closest connectable circle once we use multiple circles per node
            var from = new FeatureCircle(nodes[tile], 0);
            var to = new FeatureCircle(nodes[neighborTile], 0);

            var (r1, r2) = (from.Circle.Radius, to.Circle.Radius);
            var maxRadius = (Math.Min(r1.NumericValue, r2.NumericValue) - 1).Clamped(0, 3).U();
            var radius = random.NextFloat().Sqrted() * maxRadius;

            var trySplit = random.NextBool(0.2);

            if (node.MacroFeatures.TryGetValue(dir, out var macroFeature) && macroFeature is Crevice)
            {
                trySplit = true;
                if (radius <= 2.U())
                    radius = 0.U();
            }

            var connection = new PhysicalFeature.Connection(from, to, radius, trySplit);

            connections.Add(connection);
        }
    }

    private List<PhysicalFeature.Crevice> createCreviceFeatures(LogicalTilemap logicalTilemap)
    {
        var crevices = new List<PhysicalFeature.Crevice>();
        var seenEdges = new HashSet<TileEdge>();
        foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
        {
            var node = logicalTilemap[tile];
            foreach (var (direction, feature) in node.MacroFeatures)
            {
                if (feature is not Crevice)
                    continue;
                var edge = tile.Edge(direction);
                if (!seenEdges.Add(edge))
                    continue;

                // TODO: don't use a local constant but a converter method in global constants or similar
                const float toOuterRadius = 2 / 1.73205080757f;
                var p1 = Position2.Zero +
                    (Level.GetPosition(tile).NumericValue * 2 + direction.CornerBefore() * toOuterRadius) *
                    defaultNodeRadius;
                var p1To2 = (direction.CornerAfter() - direction.CornerBefore()) * toOuterRadius * defaultNodeRadius;

                // TODO: make this depend on length
                const int count = 5;

                var circles = Enumerable.Range(0, count).Select(i =>
                {
                    var p = p1 + p1To2 * (i / (float) (count - 1));
                    // TODO: make this radius depend on scripts
                    return new Circle(p, 1.5.U());
                });

                var creviceFeature = new PhysicalFeature.Crevice(ImmutableArray.CreateRange(circles));

                crevices.Add(creviceFeature);
            }
        }

        return crevices;
    }
}
