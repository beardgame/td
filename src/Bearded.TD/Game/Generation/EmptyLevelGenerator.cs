using System.Collections.Generic;
using Bearded.TD.Game.Commands.LevelGeneration;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation
{
    sealed class EmptyLevelGenerator : ILevelGenerator
    {
        public IEnumerable<CommandFactory> Generate(LevelGenerationParameters parameters, int seed)
        {
            var tilemap = new Tilemap<TileGeometry>(parameters.Radius, _ => new TileGeometry(TileType.Floor, 1, Unit.Zero));

            var drawInfos = TileDrawInfo.DrawInfosFromTypes(tilemap);

            yield return game => FillTilemap.Command(game, tilemap, drawInfos);

            foreach (var cmd in LegacySpawnLocationPlacer.SpawnLocations(parameters.Radius))
                yield return cmd;
        }
    }
}
