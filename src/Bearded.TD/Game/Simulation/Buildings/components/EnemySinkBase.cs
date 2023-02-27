using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

abstract class EnemySinkBase : Component, IEnemySink, IListener<Materialized>, IListener<ObjectDeleting>
{
    private ITilePresence? tilePresence;

    protected IEnumerable<Tile> OccupiedTiles => tilePresence?.OccupiedTiles ?? ImmutableArray<Tile>.Empty;

    protected override void OnAdded()
    {
        Events.Subscribe<Materialized>(this);
        Events.Subscribe<ObjectDeleting>(this);
    }

    public override void Activate()
    {
        base.Activate();
        tilePresence = Owner.GetTilePresence();
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
        Owner.GetTilePresence().ObserveChanges(AddSink, RemoveSink);

        materialize();
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        cleanUp();
    }

    private void materialize()
    {
        foreach (var tile in OccupiedTiles)
        {
            AddSink(tile);
        }
        Register();
    }

    private void cleanUp()
    {
        Unregister();
        foreach (var tile in OccupiedTiles)
        {
            RemoveSink(tile);
        }
    }

    public override void Update(TimeSpan elapsedTime) { }
}

interface IEnemySink {}
