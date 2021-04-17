using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation
{
    sealed record LogicalNode(NodeBlueprint? Blueprint, Directions ConnectedTo,
        ImmutableDictionary<Direction, MacroFeature> MacroFeatures)
    {
    }

    sealed record EdgeFeatures(bool IsConnected, MacroFeature? Feature)
    {
        public static EdgeFeatures Default { get; } = new(false, null);
    }

    record MacroFeature;

    record Crevice : MacroFeature;

    sealed class LogicalTilemap
    {
        record TileNode(NodeBlueprint? Node, EdgeFeatures Right, EdgeFeatures UpRight, EdgeFeatures UpLeft)
            : IModifiableTileEdges<TileNode, EdgeFeatures>
        {
            public static TileNode Default { get; } =
                new(null, EdgeFeatures.Default, EdgeFeatures.Default, EdgeFeatures.Default);

            public static TileNode DefaultWith(NodeBlueprint? node) =>
                node == null ? Default : Default with {Node = node};

            public TileNode WithRight(EdgeFeatures data) => this with {Right = data};
            public TileNode WithUpRight(EdgeFeatures data) => this with {UpRight = data};
            public TileNode WithUpLeft(EdgeFeatures data) => this with {UpLeft = data};
        }

        private readonly Tilemap<TileNode> tiles;

        public int Radius { get; }

        public static LogicalTilemap From(Tilemap<NodeBlueprint?> nodes,
            IReadOnlyDictionary<TileEdge, MacroFeature> macroFeatures)
        {
            var tiles = new Tilemap<TileNode>(nodes.Radius + 1,
                tile =>
                {
                    var node = nodes.IsValidTile(tile) ? nodes[tile] : null;

                    return new TileNode(
                        node,
                        edgeFeatures(tile, Direction.Right),
                        edgeFeatures(tile, Direction.UpRight),
                        edgeFeatures(tile, Direction.UpLeft));
                });

            return new(tiles);

            EdgeFeatures edgeFeatures(Tile tile, Direction direction)
                => new(false, macroFeatures.GetValueOrDefault(TileEdge.From(tile, direction)));
        }

        private LogicalTilemap(Tilemap<TileNode> tiles)
        {
            Radius = tiles.Radius - 1;

            this.tiles = tiles;
        }

        public bool IsValidTile(Tile tile) => tile.Radius <= Radius;

        public LogicalNode this[Tile tile] =>
            new(
                tiles[tile].Node,
                Extensions.Directions
                    .Where(d => this[tile.Edge(d)].IsConnected)
                    .Aggregate(Directions.None, (directions, direction) => directions.And(direction)),
                Extensions.Directions
                    .Where(d => this[tile.Edge(d)].Feature != null)
                    .ToImmutableDictionary(d => d, d => this[tile.Edge(d)].Feature));

        public void InvertConnectivity(TileEdge edge)
        {
            var features = this[edge];
            var newFeatures = features with {IsConnected = !features.IsConnected};
            setEdgeFeatures(edge, newFeatures);
        }

        public void SwapNodes((Tile, Tile) tiles) => SwapNodes(tiles.Item1, tiles.Item2);

        public void SwapNodes(Tile tile1, Tile tile2)
        {
            var node1 = tiles[tile1];
            var node2 = tiles[tile2];

            tiles[tile1] = node1 with {Node = node2.Node};
            tiles[tile2] = node2 with {Node = node1.Node};
        }

        public void SwitchMacroFeatures(Tile tile, Direction direction1, Direction direction2)
        {
            var edge1 = TileEdge.From(tile, direction1);
            var edge2 = TileEdge.From(tile, direction2);

            var feature1 = this[edge1];
            var feature2 = this[edge2];

            setEdgeFeatures(edge1, feature1 with {Feature = feature2.Feature});
            setEdgeFeatures(edge2, feature2 with {Feature = feature1.Feature});
        }

        private void setEdgeFeatures(TileEdge edge, EdgeFeatures features)
        {
            edge.ModifyEdgeIn(tiles, features);
        }

        public EdgeFeatures this[TileEdge edge] => edge.GetEdgeFrom<TileNode, EdgeFeatures>(tiles);

        public LogicalTilemap Clone() => new(tiles.Clone());
    }
}
