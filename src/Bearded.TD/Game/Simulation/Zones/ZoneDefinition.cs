using System.Collections.Immutable;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Zones
{
    sealed record ZoneDefinition(Id<Zone> Id, ImmutableArray<Tile> Tiles);
}
