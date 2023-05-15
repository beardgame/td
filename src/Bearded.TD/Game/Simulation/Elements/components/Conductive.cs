using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("conductive")]
sealed class Conductive : Component
{
    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        Owner.GetTilePresence().ObserveChanges(
            addedTile => Owner.Game.ConductiveLayer.AddObjectToTile(Owner, addedTile),
            removedTile => Owner.Game.ConductiveLayer.RemoveObjectFromTile(Owner, removedTile));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
