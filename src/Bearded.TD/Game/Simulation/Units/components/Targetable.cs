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
        Owner.TrackTilePresenceInLayer(Owner.Game.TargetLayer);
    }

    public override void Update(TimeSpan elapsedTime) { }
}
