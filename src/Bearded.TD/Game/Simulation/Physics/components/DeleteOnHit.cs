using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("deleteOnHit")]
sealed class DeleteOnHit : Component, IListener<CollideWithObject>, IListener<CollideWithLevel>
{
    protected override void OnAdded()
    {
        Events.Subscribe<CollideWithObject>(this);
        Events.Subscribe<CollideWithLevel>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(CollideWithObject e)
    {
        Owner.Delete();
    }

    public void HandleEvent(CollideWithLevel e)
    {
        Owner.Delete();
    }
}
