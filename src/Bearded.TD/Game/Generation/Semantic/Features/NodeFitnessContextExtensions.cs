using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Generation.Semantic.Features;

static class NodeFitnessContextExtensions
{
    public static IEnumerable<PlacedNode> ConnectedNodes(this INodeFitnessContext context, Tile nodeTile)
    {
        return context.ConnectedTiles(nodeTile).Select(t => context[t]);
    }

    public static IEnumerable<Tile> ConnectedTiles(this INodeFitnessContext context, Tile nodeTile)
    {
        var node = context[nodeTile];
        return Extensions.Directions
            .Where(d => node!.ConnectedTo.Includes(d))
            .Select(nodeTile.Neighbor);
    }

    public static IEnumerable<PlacedNode> NodesReachableInSteps(
        this INodeFitnessContext context, Tile nodeTile, int steps)
    {
        var seen = new HashSet<Tile> { nodeTile };

        var q = new Queue<(Tile Tile, int Steps)>();
        q.Enqueue((nodeTile, steps));

        while (q.TryDequeue(out var tuple))
        {
            var current = tuple.Tile;
            var stepsLeft = tuple.Steps;

            foreach (var neighbor in context.ConnectedTiles(current).WhereNot(seen.Contains))
            {
                seen.Add(neighbor);
                yield return context[neighbor];
                if (stepsLeft > 1)
                {
                    q.Enqueue((neighbor, stepsLeft - 1));
                }
            }
        }
    }
}
