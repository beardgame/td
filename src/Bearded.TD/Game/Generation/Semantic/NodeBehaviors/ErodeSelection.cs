using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("erodeSelection")]
    sealed class ErodeSelection : NodeBehavior<ErodeSelection.BehaviorParameters>
    {
        public sealed record BehaviorParameters(int? Strength);

        public ErodeSelection(BehaviorParameters parameters) : base(parameters) { }

        public override void Generate(NodeGenerationContext context)
        {
            var eroded = context.Tiles.Selection.Erode(Parameters.Strength ?? 6);
            context.Tiles.Selection.RemoveAll();
            foreach (var tile in eroded)
            {
                context.Tiles.Selection.Add(tile);
            }
        }
    }
}
