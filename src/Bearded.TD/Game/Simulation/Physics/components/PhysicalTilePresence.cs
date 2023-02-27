using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("physicalTilePresence")]
sealed class PhysicalTilePresence : Component
{
    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        Owner.GetTilePresence().ObserveChanges(
            addedTile => Owner.Game.PhysicsLayer.AddObjectToTile(Owner, addedTile),
            removedTile => Owner.Game.PhysicsLayer.RemoveObjectFromTile(Owner, removedTile));
    }

    public override void Update(TimeSpan elapsedTime) {}
}
