using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;
using Extensions = Bearded.Utilities.Linq.Extensions;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

using static ObjectPlacement.PlacementMode;

static class ObjectPlacement
{
    public enum PlacementMode
    {
        FirstFromSelection = 0,
        AllFromSelection,
        AwayFromConnections,
        RandomTile,
    }

    public static IEnumerable<Tile> ToTiles(
        this PlacementMode mode,
        NodeGenerationContext context,
        int? count,
        Predicate<Tile>? filter)
    {
        return mode switch
        {
            FirstFromSelection => single(
                context.Tiles.Selection.Count > 0
                    ? context.Tiles.Selection.First()
                    : Level.GetTile(context.NodeData.Circles[0].Center)),

            AllFromSelection => context.Tiles.Selection,

            AwayFromConnections => single(
                context.Tiles.All.Where(tile => filter?.Invoke(tile) ?? true)
                    .MaxBy(t => context.NodeData.Connections.Sum(c => c.DistanceTo(t)))),

            RandomTile => multiple(
                () => Extensions.RandomElement(context.Tiles.Selection, context.Random),
                count),

            _ => throw new ArgumentOutOfRangeException($"Unhandled placement mode: {mode}")
        };
    }

    private static IEnumerable<Tile> single(Tile tile) => Extensions.Yield(tile);

    private static IEnumerable<Tile> multiple(Func<Tile> tileSelector, int? count) =>
        Enumerable.Range(0, count ?? 1).Select(_ => tileSelector());
}
