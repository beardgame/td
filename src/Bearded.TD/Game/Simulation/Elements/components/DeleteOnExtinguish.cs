using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("deleteOnExtinguish")]
class DeleteOnExtinguish : Component,  IListener<FireExtinguished>
{
    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(FireExtinguished @event)
    {
        Owner.Delete();
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
