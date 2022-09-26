using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Zones;

sealed record Zone(
        Id<Zone> Id, ImmutableArray<Tile> CoreTiles, ImmutableArray<Tile> ExtendedVisibilityTiles, bool Explorable)
    : IIdable<Zone>
{
    public IEnumerable<Tile> VisibilityTiles => CoreTiles.Concat(ExtendedVisibilityTiles);
}
