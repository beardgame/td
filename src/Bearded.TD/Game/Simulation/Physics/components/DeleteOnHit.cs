using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("deleteOnHit")]
sealed class DeleteOnHit : Component, IListener<HitObject>, IListener<HitLevel>
{
    protected override void OnAdded()
    {
        Events.Subscribe<HitObject>(this);
        Events.Subscribe<HitLevel>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(HitObject e)
    {
        Owner.Delete();
    }

    public void HandleEvent(HitLevel e)
    {
        Owner.Delete();
    }
}
