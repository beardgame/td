using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("forceToCenter")]
    sealed class ForceToCenter : NodeBehavior
    {
        public override double GetFitnessPenalty(INodeFitnessContext context, Tile nodeTile)
        {
            return nodeTile.Radius * 1000;
        }
    }
}
