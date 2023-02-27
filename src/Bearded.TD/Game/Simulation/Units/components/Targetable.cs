using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

[Component("targetable")]
sealed class Targetable : Component
{
    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        Owner.GetTilePresence().ObserveChanges(
            addedTile => Owner.Game.TargetLayer.AddObjectToTile(Owner, addedTile),
            removedTile => Owner.Game.TargetLayer.AddObjectToTile(Owner, removedTile));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
