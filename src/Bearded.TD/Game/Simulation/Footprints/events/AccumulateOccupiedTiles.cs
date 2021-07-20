using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Footprints
{
    readonly struct AccumulateOccupiedTiles : IComponentEvent
    {
        private readonly Accumulator accumulator;

        public AccumulateOccupiedTiles(Accumulator accumulator)
        {
            this.accumulator = accumulator;
        }

        public void AddTiles(IEnumerable<Tile> tiles) => tiles.ForEach(AddTile);

        public void AddTile(Tile tile) => accumulator.AddTile(tile);

        public sealed class Accumulator
        {
            private readonly HashSet<Tile> tiles = new();

            public void AddTile(Tile tile)
            {
                tiles.Add(tile);
            }

            public ImmutableHashSet<Tile> ToTileSet() => tiles.ToImmutableHashSet();
        }
    }
}
