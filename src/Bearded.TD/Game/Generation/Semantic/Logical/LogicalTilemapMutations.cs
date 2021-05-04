using System;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Extensions = Bearded.TD.Tiles.Extensions;

namespace Bearded.TD.Game.Generation.Semantic.Logical
{
    interface ILogicalTilemapMutation
    {
        bool TryMutate(LogicalTilemap tilemap, Random random);
    }

    static class LogicalTilemapMutations
    {
        public sealed class SwapMacroFeatures : ILogicalTilemapMutation
        {
            public bool TryMutate(LogicalTilemap tilemap, Random random)
            {
                var tile = Tilemap.EnumerateTilemapWith(tilemap.Radius).RandomElement(random);
                var node = tilemap[tile];
                if (node.MacroFeatures.IsEmpty)
                {
                    return false;
                }

                var direction1 = node.MacroFeatures.Keys.RandomElement(random);
                var direction2 = Extensions.Directions.Except(direction1.Yield()).RandomElement(random);

                tilemap.SwitchMacroFeatures(tile, direction1, direction2);
                return true;
            }
        }

        public sealed class ToggleConnection : ILogicalTilemapMutation
        {
            public bool TryMutate(LogicalTilemap tilemap, Random random)
            {
                return LogicalTilemapOptimizationMutator.TryCallOnConnectedTilesWithBlueprint(
                    tilemap, tilemap.InvertConnectivity, random);
            }
        }

        public sealed class SwapNodes : ILogicalTilemapMutation
        {
            public bool TryMutate(LogicalTilemap tilemap, Random random)
            {
                return LogicalTilemapOptimizationMutator.TryCallOnConnectedTilesWithBlueprint(
                    tilemap, e => tilemap.SwapNodes(e.AdjacentTiles), random);
            }
        }
    }
}
