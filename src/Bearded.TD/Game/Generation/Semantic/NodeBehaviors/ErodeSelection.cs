using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("erodeSelection")]
    sealed class ErodeSelection : NodeBehavior
    {
        public override void Generate(NodeGenerationContext context)
        {
            var tilesToRemove = new List<Tile>();
            foreach (var tile in context.Tiles.Selection)
            {
                var allNeighborsSelected = tile
                    .PossibleNeighbours()
                    .All(context.Tiles.Selection.Contains);

                if(!allNeighborsSelected)
                    tilesToRemove.Add(tile);
            }

            foreach (var tile in tilesToRemove)
            {
                context.Tiles.Selection.Remove(tile);
            }
        }
    }
}
