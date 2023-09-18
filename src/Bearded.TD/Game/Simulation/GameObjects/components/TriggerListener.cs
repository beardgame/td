using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class TriggerListener : Component
{
    private readonly ImmutableArray<ITrigger> triggers;
    private readonly List<ITriggerSubscription> subscriptions = new();
    private readonly Action callback;

    public TriggerListener(ImmutableArray<ITrigger> triggers, Action callback)
    {
        this.triggers = triggers;
        this.callback = callback;
    }

    protected override void OnAdded()
    {
        foreach (var trigger in triggers)
        {
            subscriptions.Add(trigger.Subscribe(Events, callback));
        }
    }

    public override void OnRemoved()
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Unsubscribe(Events);
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

