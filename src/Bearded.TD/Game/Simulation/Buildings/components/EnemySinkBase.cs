using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

abstract class EnemySinkBase : Component, IEnemySink, IListener<Materialized>, IListener<ObjectDeleting>
{
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();

    protected override void OnAdded()
    {
        Events.Subscribe<Materialized>(this);
        Events.Subscribe<ObjectDeleting>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<Materialized>(this);
        Events.Unsubscribe<ObjectDeleting>(this);

        foreach (var tile in occupiedTilesTracker.OccupiedTiles)
        {
            RemoveSink(tile);
        }
        occupiedTilesTracker.Dispose(Events);
    }

    protected abstract void AddSink(Tile t);
    protected abstract void RemoveSink(Tile t);

    public void HandleEvent(Materialized @event)
    {
        occupiedTilesTracker.Initialize(Owner, Events);

        foreach (var tile in occupiedTilesTracker.OccupiedTiles)
        {
            AddSink(tile);
        }

        occupiedTilesTracker.TileAdded += AddSink;
        occupiedTilesTracker.TileRemoved += RemoveSink;
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        foreach (var tile in occupiedTilesTracker.OccupiedTiles)
        {
            RemoveSink(tile);
        }
    }

    public override void Update(TimeSpan elapsedTime) { }
}

interface IEnemySink {}
