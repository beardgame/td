using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("selectAll")]
    sealed class SelectAll : NodeBehavior
    {
        public override void Generate(NodeGenerationContext context)
        {
            context.Tiles.Selection.Reset();
        }
    }
}
