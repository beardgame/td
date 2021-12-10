using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("dilateSelection")]
sealed class DilateSelection : NodeBehavior
{
    public override void Generate(NodeGenerationContext context)
    {
        var allNeighbors = new HashSet<Tile>();
        foreach (var tile in context.Tiles.Selection)
        {
            foreach (var neighbor in tile.PossibleNeighbours())
            {
                allNeighbors.Add(neighbor);
            }
        }

        var validNeighbors = allNeighbors.Where(tile => context.Tiles.All.Contains(tile));

        foreach (var tile in validNeighbors)
        {
            context.Tiles.Selection.Add(tile);
        }
    }
}