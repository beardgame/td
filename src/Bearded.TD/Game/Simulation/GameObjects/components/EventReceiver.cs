using Bearded.TD.Game.Simulation.Damage;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IEventReceiver<in TEvent> where TEvent : struct, IComponentEvent
{
    void InjectEvent(TEvent e);
}

class EventReceiver<TEvent> : Component, IEventReceiver<TEvent>
    where TEvent : struct, IComponentEvent
{
    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void InjectEvent(TEvent e)
    {
        Events.Send(e);
    }
}

[Component("takeHitEventReceiver")]
sealed class TakeHitEventReceiver : EventReceiver<TakeHit> {}
