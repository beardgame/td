using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("tagSelected")]
    sealed class TagSelected : NodeBehavior<TagSelected.BehaviorParameters>
    {
        public record BehaviorParameters(string? Tag, double Value = 1);

        public TagSelected(BehaviorParameters parameters) : base(parameters) { }

        public override void Generate(NodeGenerationContext context)
        {
            foreach (var tile in context.Tiles.Selection)
            {
                context.Tiles.Tags[Parameters.Tag ?? "default"][tile] += Parameters.Value;
            }
        }
    }
}
