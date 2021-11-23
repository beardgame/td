using System.Collections.Immutable;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Zones
{
    sealed record Zone(Id<Zone> Id, ImmutableArray<Tile> Tiles) : IIdable<Zone>;
}
