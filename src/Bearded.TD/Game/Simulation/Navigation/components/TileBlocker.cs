using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Navigation;

[Component("tileBlocker")]
sealed class TileBlocker : Component, IListener<ObjectDeleting>
{
    private ITilePresenceListener? tilePresenceListener;

    protected override void OnAdded() { }

    public override void Activate()
    {
        tilePresenceListener = Owner.GetTilePresence().ObserveChanges(
            onAdded: tile => Owner.Game.TileBlockerLayer.AddTileBlocker(Owner, tile),
            onRemoved: tile => Owner.Game.TileBlockerLayer.RemoveTileBlocker(Owner, tile));
    }

    public override void OnRemoved()
    {
        detachTileListener();
        base.OnRemoved();
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        detachTileListener();
    }

    private void detachTileListener()
    {
        tilePresenceListener?.Detach();
        tilePresenceListener = null;
    }

    public override void Update(TimeSpan elapsedTime) { }
}
