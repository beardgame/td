using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("resetTagSelected")]
sealed class ResetTagSelected : NodeBehavior<ResetTagSelected.BehaviorParameters>
{
    public sealed record BehaviorParameters(string? Tag, double Value = 0);

    public ResetTagSelected(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        foreach (var tile in context.Tiles.Selection)
        {
            context.Tiles.Tags[Parameters.Tag ?? "default"][tile] = Parameters.Value;
        }
    }
}