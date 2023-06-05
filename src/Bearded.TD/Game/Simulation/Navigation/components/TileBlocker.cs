using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Navigation;

[Component("tileBlocker")]
sealed class TileBlocker : Component, IListener<ObjectDeleting>
{
    private ITilePresenceListener? tileBlockerLayerPresence;

    protected override void OnAdded() { }

    public override void Activate()
    {
        tileBlockerLayerPresence = Owner.GetTilePresence().ObserveChanges(
            onAdded: tile => Owner.Game.TileBlockerLayer.AddTileBlocker(Owner, tile),
            onRemoved: tile => Owner.Game.TileBlockerLayer.RemoveTileBlocker(Owner, tile));
    }

    public override void OnRemoved()
    {
        detachFromTileBlockerLayer();
        base.OnRemoved();
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        detachFromTileBlockerLayer();
    }

    private void detachFromTileBlockerLayer()
    {
        tileBlockerLayerPresence?.Detach();
        tileBlockerLayerPresence = null;
    }

    public override void Update(TimeSpan elapsedTime) { }
}
