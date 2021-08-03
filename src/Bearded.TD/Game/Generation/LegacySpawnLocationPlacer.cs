using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation
{
    static class LegacySpawnLocationPlacer
    {
        public static IEnumerable<CommandFactory> SpawnLocations(int radius)
        {
            return
                from dir in Directions.All.Enumerate()
                where dir != Direction.Unknown
                select Tile.Origin + dir.Step() * radius into tile
                select (CommandFactory)
                    (game => CreateSpawnLocation.Command(game, game.Ids.GetNext<SpawnLocation>(), tile));
        }
    }
}
