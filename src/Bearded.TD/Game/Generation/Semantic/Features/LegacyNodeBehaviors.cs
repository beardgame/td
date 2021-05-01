using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using Extensions = Bearded.TD.Tiles.Extensions;
using Tile = Bearded.TD.Tiles.Tile;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    // TODO: set these up to be loaded from mods
    sealed class BaseNodeBehavior : INodeBehavior
    {
        public ImmutableArray<NodeTag> Tags { get; } = ImmutableArray.Create(new NodeTag("base"));
    }

    sealed class ForceToCenter : INodeBehavior
    {
        public double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile)
        {
            return nodeTile.Radius * 1000;
        }
    }

    sealed class SpawnerNodeBehavior : INodeBehavior
    {
        public ImmutableArray<NodeTag> Tags { get; } = ImmutableArray.Create(new NodeTag("spawner"));
    }

    sealed class DontBeAdjacentToTag : INodeBehavior
    {
        private readonly NodeTag tagToAvoid;

        public DontBeAdjacentToTag(NodeTag tagToAvoid)
        {
            this.tagToAvoid = tagToAvoid;
        }

        public double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile)
        {
            var node = context[nodeTile];

            var connectedNodes = Extensions.Directions
                .Where(d => node!.ConnectedTo.Includes(d))
                .Select(nodeTile.Neighbour)
                .Select(t => context[t]);

            return connectedNodes.Count(n => n.Blueprint!.Tags.Contains(tagToAvoid)) * 100;
        }
    }

    sealed class ClearAllTiles : INodeBehavior
    {
        public void Generate(NodeGenerationContext context)
        {
            foreach (var t in context.Tiles)
            {
                context.Set(t, new TileGeometry(TileType.Floor, 0, 0.U()));
            }
        }
    }
}
