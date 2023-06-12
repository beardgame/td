using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

internal interface IRanger
{
    ImmutableArray<Tile> GetTilesInRange(
        GameState game,
        PassabilityLayer passabilityLayer,
        Tile origin,
        Unit minimumRange,
        Unit maximumRange);
}
