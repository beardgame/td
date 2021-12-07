using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints
{
    sealed class MovableTileOccupation<T> : TileOccupationBase<T>
    {
        private PositionedFootprint footprint;

        public override IEnumerable<Tile> OccupiedTiles => footprint.OccupiedTiles;

        public void SetFootprint(PositionedFootprint newFootprint)
        {
            var oldFootprint = footprint;
            footprint = newFootprint;

            var oldTiles = oldFootprint.OccupiedTiles.ToImmutableHashSet();
            var newTiles = newFootprint.OccupiedTiles.ToImmutableHashSet();

            var removedTiles = oldTiles.Except(newTiles);
            var addedTiles = newTiles.Except(oldTiles);

            removedTiles.Select(t => new TileLeft(t)).ForEach(Events.Send);
            addedTiles.Select(t => new TileEntered(t)).ForEach(Events.Send);
            Events.Send(new FootprintChanged(newFootprint));
        }

        public override void Update(TimeSpan elapsedTime) {}
    }
}
