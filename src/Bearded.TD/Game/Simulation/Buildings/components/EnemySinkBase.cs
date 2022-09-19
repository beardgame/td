using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

abstract class EnemySinkBase : Component, IEnemySink, IListener<Materialized>, IListener<ObjectDeleting>
{
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();

    protected ReadOnlyCollection<Tile> OccupiedTiles => occupiedTilesTracker.OccupiedTiles;

    protected override void OnAdded()
    {
        Events.Subscribe<Materialized>(this);
        Events.Subscribe<ObjectDeleting>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<Materialized>(this);
        Events.Unsubscribe<ObjectDeleting>(this);

        cleanUp();
    }

    protected abstract void AddSink(Tile t);
    protected abstract void RemoveSink(Tile t);
    protected virtual void Register() {}
    protected virtual void Unregister() {}

    public void HandleEvent(Materialized @event)
    {
        occupiedTilesTracker.TileAdded += AddSink;
        occupiedTilesTracker.TileRemoved += RemoveSink;

        materialize();
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        cleanUp();
    }

    private void materialize()
    {
        occupiedTilesTracker.Initialize(Owner, Events);
        foreach (var tile in occupiedTilesTracker.OccupiedTiles)
        {
            AddSink(tile);
        }
        Register();
    }

    private void cleanUp()
    {
        Unregister();
        foreach (var tile in occupiedTilesTracker.OccupiedTiles)
        {
            RemoveSink(tile);
        }
        occupiedTilesTracker.Dispose(Events);
    }

    public override void Update(TimeSpan elapsedTime) { }
}

interface IEnemySink {}
