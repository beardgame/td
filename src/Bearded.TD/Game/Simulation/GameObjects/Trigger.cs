using System;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class Trigger<T> : ITrigger where T : struct, IComponentEvent
{
    public ITriggerSubscription Subscribe(ComponentEvents events, Action action)
    {
        var sub = new TriggerSubscription<T>(action);
        events.Subscribe(sub);
        return sub;
    }
}
