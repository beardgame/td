using System.Collections.Immutable;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Zones
{
    sealed class Zone : IIdable<Zone>
    {
        public Id<Zone> Id { get; }
        public ImmutableArray<Tile> Tiles { get; }

        public Zone(Id<Zone> id, ImmutableArray<Tile> tiles)
        {
            Id = id;
            Tiles = tiles;
        }
    }
}
