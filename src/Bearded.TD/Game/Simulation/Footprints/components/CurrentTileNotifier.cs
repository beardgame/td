using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints;

[Component("currentTileNotifier")]
sealed class CurrentTileNotifier : Component , IListener<ObjectDeleting>
{
    private Tile lastKnownTile;
    private bool activated;

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        lastKnownTile = Level.GetTile(Owner.Position);
        Events.Send(new TileEntered(lastKnownTile));
        Events.Subscribe(this);
        activated = true;
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
        if (activated)
        {
            Events.Send(new TileLeft(lastKnownTile));
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Owner.Deleted)
            return;

        var tile = Level.GetTile(Owner.Position);
        if (tile == lastKnownTile)
            return;

        Events.Send(new TileLeft(lastKnownTile));
        Events.Send(new TileEntered(tile));
        lastKnownTile = tile;
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        Events.Send(new TileLeft(lastKnownTile));
    }
}
