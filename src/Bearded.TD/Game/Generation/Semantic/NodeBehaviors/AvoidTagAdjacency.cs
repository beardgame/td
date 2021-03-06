using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("avoidTagAdjacency")]
    sealed class AvoidTagAdjacency : NodeBehavior<AvoidTagAdjacency.BehaviorParameters>
    {
        public AvoidTagAdjacency(BehaviorParameters parameters) : base(parameters) { }

        public override double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile)
        {
            var node = context[nodeTile];

            var connectedNodes = Extensions.Directions
                .Where(d => node!.ConnectedTo.Includes(d))
                .Select(nodeTile.Neighbour)
                .Select(t => context[t]);

            return connectedNodes.Count(n => n.Blueprint!.AllTags.Contains(Parameters.TagToAvoid)) * 100;
        }

        public sealed record BehaviorParameters(NodeTag TagToAvoid);
    }
}
