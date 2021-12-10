using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Footprints;

sealed class OccupiedTilesTracker : IListener<TileEntered>, IListener<TileLeft>
{
    private readonly List<Tile> occupiedTiles = new();
    public ReadOnlyCollection<Tile> OccupiedTiles { get; }

    public event GenericEventHandler<Tile>? TileAdded;
    public event GenericEventHandler<Tile>? TileRemoved;

    public OccupiedTilesTracker()
    {
        OccupiedTiles = occupiedTiles.AsReadOnly();
    }

    public void Initialize(IComponentOwner owner, ComponentEvents events)
    {
        State.Satisfies(occupiedTiles.Count == 0);

        occupiedTiles.AddRange(OccupiedTileAccumulator.AccumulateOccupiedTiles(owner));
        events.Subscribe<TileEntered>(this);
        events.Subscribe<TileLeft>(this);
    }

    public void Dispose(ComponentEvents events)
    {
        events.Unsubscribe<TileEntered>(this);
        events.Unsubscribe<TileLeft>(this);
    }

    public void HandleEvent(TileEntered @event)
    {
        if (!occupiedTiles.Contains(@event.Tile))
        {
            occupiedTiles.Add(@event.Tile);
            TileAdded?.Invoke(@event.Tile);
        }
    }

    public void HandleEvent(TileLeft @event)
    {
        if (occupiedTiles.RemoveAll(t => t == @event.Tile) > 0)
        {
            TileRemoved?.Invoke(@event.Tile);
        }
    }
}