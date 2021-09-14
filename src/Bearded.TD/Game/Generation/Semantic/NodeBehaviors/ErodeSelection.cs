using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("erodeSelection")]
    sealed class ErodeSelection : NodeBehavior<ErodeSelection.BehaviorParameters>
    {
        public sealed record BehaviorParameters(int Strength = 6);

        public ErodeSelection(BehaviorParameters parameters) : base(parameters) { }

        public override void Generate(NodeGenerationContext context)
        {
            if (Parameters.Strength == 0)
                return;

            var tilesToRemove = new List<Tile>();
            foreach (var tile in context.Tiles.Selection)
            {
                var selectedNeighbors = tile
                    .PossibleNeighbours()
                    .Count(context.Tiles.Selection.Contains);

                if(selectedNeighbors < Parameters.Strength)
                    tilesToRemove.Add(tile);
            }

            foreach (var tile in tilesToRemove)
            {
                context.Tiles.Selection.Remove(tile);
            }
        }
    }
}
