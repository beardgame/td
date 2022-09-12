using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors;

sealed class FitnessTestContext : INodeFitnessContext
{
    private readonly Dictionary<Tile, Node> nodes = new();
    private readonly Dictionary<Tile, Directions> connections = new();

    public static FitnessTestContext UnconnectedWithRadius(int radius)
    {
        var ctx = new FitnessTestContext();
        foreach (var tile in Tilemap.GetOutwardSpiralForTilemapWith(radius))
        {
            ctx.nodes[tile] = emptyNode();
        }
        return ctx;
    }

    public static FitnessTestContext CreateSpiderWithRadius(int radius)
    {
        var ctx = UnconnectedWithRadius(radius);
        foreach (var dir in Extensions.Directions)
        {
            var curr = Tile.Origin;
            for (var i = 0; i < radius; i++)
            {
                ctx.Connect(curr, dir);
                curr = curr.Neighbor(dir);
            }
        }
        return ctx;
    }

    private FitnessTestContext() {}

    public void TagNode(Tile t, string tag)
    {
        var existingNode =
            nodes.GetValueOrDefault(t, emptyNode());
        nodes[t] = existingNode with { Behaviors = existingNode.Behaviors.Add(TagNodeBehaviour.FromTagString(tag)) };
    }

    public void Connect(Tile t, Direction d)
    {
        connectOneWay(t, d);
        connectOneWay(t.Neighbor(d), d.Opposite());
    }

    private void connectOneWay(Tile t, Direction d)
    {
        var connectivity = connections.GetValueOrDefault(t);
        connections[t] = connectivity.And(d);
    }

    public void Disconnect(Tile t, Direction d)
    {
        disconnectOneWay(t, d);
        disconnectOneWay(t.Neighbor(d), d.Opposite());
    }

    private void disconnectOneWay(Tile t, Direction d)
    {
        var connectivity = connections.GetValueOrDefault(t);
        connections[t] = connectivity.Except(d);
    }

    public PlacedNode this[Tile tile] => new(
        nodes.GetValueOrDefault(tile),
        connections.GetValueOrDefault(tile),
        ImmutableDictionary<Direction, MacroFeature>.Empty);

    private static Node emptyNode() => Node.FromBlueprint(new TestNodeBlueprint());

    private sealed class TagNodeBehaviour: NodeBehavior
    {
        private readonly NodeTag tag;

        public override ImmutableArray<NodeTag> Tags => ImmutableArray.Create(tag);

        public TagNodeBehaviour(NodeTag tag)
        {
            this.tag = tag;
        }

        public static TagNodeBehaviour FromTagString(string tag) => new(new NodeTag(tag));
    }
}
