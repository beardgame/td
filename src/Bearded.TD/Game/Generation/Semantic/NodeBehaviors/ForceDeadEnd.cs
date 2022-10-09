using System;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("forceDeadEnd")]
sealed class ForceDeadEnd : NodeBehavior<ForceDeadEnd.BehaviorParameters>
{
    public ForceDeadEnd(BehaviorParameters parameters) : base(parameters) { }

    public override double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile)
    {
        return Math.Max(0, context.ConnectedNodes(nodeTile).Count() - 1) * Parameters.PenaltyFactor;
    }

    [UsedImplicitly]
    public sealed record BehaviorParameters(double PenaltyFactor = 100);
}
