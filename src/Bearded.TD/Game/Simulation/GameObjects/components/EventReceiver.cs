using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IEventReceiver<in TEvent> where TEvent : struct, IComponentEvent
{
    void InjectEvent(TEvent e);
}

sealed class EventReceiver<TEvent> : Component, IEventReceiver<TEvent>
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

