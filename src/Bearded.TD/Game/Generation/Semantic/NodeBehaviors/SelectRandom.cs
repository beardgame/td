using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("selectRandom")]
sealed class SelectRandom : NodeBehavior<SelectRandom.BehaviourParameters>
{
    public record BehaviourParameters(double Percentage);

    public SelectRandom(BehaviourParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        context.Tiles.Selection.RemoveAll();

        var numberOfTilesToSelect = MoreMath.RoundToInt(context.Tiles.All.Count * Parameters.Percentage);
        var tilesToSelect = context.Tiles.All.RandomSubset(numberOfTilesToSelect, context.Random);

        foreach (var tile in tilesToSelect)
        {
            context.Tiles.Selection.Add(tile);
        }
    }
}