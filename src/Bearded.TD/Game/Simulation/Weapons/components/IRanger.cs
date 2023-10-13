using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IRanger
{
    ImmutableArray<Tile> GetTilesInRange(
        GameState game,
        PassabilityLayer passabilityLayer,
        Tile origin,
        Unit minimumRange,
        Unit maximumRange) => GetTilesInRange(game, passabilityLayer, origin.Yield(), minimumRange, maximumRange);

    ImmutableArray<Tile> GetTilesInRange(
        GameState game,
        PassabilityLayer passabilityLayer,
        IEnumerable<Tile> origin,
        Unit minimumRange,
        Unit maximumRange);
}
