using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Commands.LevelGeneration;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.Utilities;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class ZoneGenerator
    {
        public CommandFactory GenerateZones(IEnumerable<TiledFeature> tiledFeatures)
        {
            var zones = generateZones(tiledFeatures);
            return game => DefineZones.Command(game, patchWithIds(game, zones));
        }

        private static ImmutableArray<Zone> generateZones(IEnumerable<TiledFeature> tiledFeatures)
        {
            // TODO: figure out what to do with the tiles between nodes.
            return tiledFeatures
                .OfType<TiledFeature.Node>()
                .Select(node => new Zone(Id<Zone>.Invalid, node.Tiles.ToImmutableArray()))
                .ToImmutableArray();
        }

        private static ImmutableArray<Zone> patchWithIds(GameInstance game, IEnumerable<Zone> zones)
        {
            return zones.Select(zone => zone with { Id = game.Ids.GetNext<Zone>() }).ToImmutableArray();
        }
    }
}
