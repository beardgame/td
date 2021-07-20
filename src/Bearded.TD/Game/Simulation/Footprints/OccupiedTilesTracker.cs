using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Footprints
{
    sealed class OccupiedTilesTracker : IListener<TileEntered>, IListener<TileLeft>
    {
        private readonly HashSet<Tile> occupiedTiles = new();
        public IEnumerable<Tile> OccupiedTiles { get; }

        public event GenericEventHandler<Tile>? TileAdded;
        public event GenericEventHandler<Tile>? TileRemoved;

        public OccupiedTilesTracker()
        {
            OccupiedTiles = occupiedTiles.AsReadOnlyEnumerable();
        }

        public void Initialize(ComponentEvents events)
        {
            State.Satisfies(occupiedTiles.Count == 0);

            var accumulator = new AccumulateOccupiedTiles.Accumulator();
            events.Send(new AccumulateOccupiedTiles(accumulator));
            occupiedTiles.UnionWith(accumulator.ToTileSet());
            events.Subscribe<TileEntered>(this);
            events.Subscribe<TileLeft>(this);
        }

        public void HandleEvent(TileEntered @event)
        {
            if (occupiedTiles.Add(@event.Tile))
            {
                TileAdded?.Invoke(@event.Tile);
            }
        }

        public void HandleEvent(TileLeft @event)
        {
            if (occupiedTiles.Remove(@event.Tile))
            {
                TileRemoved?.Invoke(@event.Tile);
            }
        }
    }
}
