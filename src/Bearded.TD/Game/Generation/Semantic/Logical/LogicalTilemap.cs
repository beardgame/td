using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Logical;

sealed class LogicalTilemap : INodeFitnessContext
{
    private sealed record TileNode(
            Node? Node, IBiome? Biome, EdgeFeatures Right, EdgeFeatures UpRight, EdgeFeatures UpLeft)
        : IModifiableTileEdges<TileNode, EdgeFeatures>
    {
        public TileNode WithRight(EdgeFeatures data) => this with {Right = data};
        public TileNode WithUpRight(EdgeFeatures data) => this with {UpRight = data};
        public TileNode WithUpLeft(EdgeFeatures data) => this with {UpLeft = data};
    }

    public static LogicalTilemap From(
        Tilemap<Node?> nodes,
        Tilemap<IBiome> biomes,
        IReadOnlyDictionary<TileEdge, MacroFeature> macroFeatures)
    {
        var tiles = new Tilemap<TileNode>(
            nodes.Radius + 1,
            tile =>
            {
                var node = nodes.IsValidTile(tile) ? nodes[tile] : null;
                var biome = biomes.IsValidTile(tile) ? biomes[tile] : null;

                return new TileNode(
                    node,
                    biome,
                    edgeFeatures(tile, Direction.Right),
                    edgeFeatures(tile, Direction.UpRight),
                    edgeFeatures(tile, Direction.UpLeft));
            });

        return new LogicalTilemap(tiles);

        EdgeFeatures edgeFeatures(Tile tile, Direction direction) =>
            new(false, macroFeatures.GetValueOrDefault(TileEdge.From(tile, direction)));
    }

    private readonly Tilemap<TileNode> tiles;

    public int Radius { get; }

    private LogicalTilemap(Tilemap<TileNode> tiles)
    {
        this.tiles = tiles;
        Radius = tiles.Radius - 1;
    }

    public bool IsValidTile(Tile tile) => tile.Radius <= Radius;

    public PlacedNode this[Tile tile] =>
        new(
            tiles[tile].Node,
            tiles[tile].Biome,
            Extensions.Directions
                .Where(d => this[tile.Edge(d)].IsConnected)
                .Aggregate(Directions.None, (directions, direction) => directions.And(direction)),
            Extensions.Directions
                .Where(d => this[tile.Edge(d)].Feature != null)
                .ToImmutableDictionary(d => d, d => this[tile.Edge(d)].Feature));

    public EdgeFeatures this[TileEdge edge] => edge.GetEdgeFrom<TileNode, EdgeFeatures>(tiles);

    public void InvertConnectivity(TileEdge edge)
    {
        var features = this[edge];
        var newFeatures = features with {IsConnected = !features.IsConnected};
        setEdgeFeatures(edge, newFeatures);
    }

    public void SwapNodes((Tile, Tile) tilePair) => SwapNodes(tilePair.Item1, tilePair.Item2);

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

    public LogicalTilemap Clone() => new(tiles.Clone());
}
