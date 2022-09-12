using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("avoidTagProximity")]
sealed class AvoidTagProximity : NodeBehavior<AvoidTagProximity.BehaviorParameters>
{
    public AvoidTagProximity(BehaviorParameters parameters) : base(parameters) { }

    public override double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile)
    {
        return context
            .NodesReachableInSteps(nodeTile, Parameters.Steps)
            .Count(
                n => n.Blueprint!.AllTags.Contains(Parameters.TagToAvoid)) * Parameters.PenaltyFactor;
    }

    [UsedImplicitly]
    public sealed record BehaviorParameters(NodeTag TagToAvoid, int Steps, int PenaltyFactor = 100);
}
