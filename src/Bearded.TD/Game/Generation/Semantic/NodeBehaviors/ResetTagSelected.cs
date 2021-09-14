using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("tagSelected")]
    sealed class ResetTagSelected : NodeBehavior<ResetTagSelected.BehaviorParameters>
    {
        public record BehaviorParameters(string? Tag, double Value = 0);

        public ResetTagSelected(BehaviorParameters parameters) : base(parameters) { }

        public override void Generate(NodeGenerationContext context)
        {
            foreach (var tile in context.Tiles.Selection)
            {
                context.Tiles.Tags[Parameters.Tag ?? "default"][tile] = Parameters.Value;
            }
        }
    }
}
