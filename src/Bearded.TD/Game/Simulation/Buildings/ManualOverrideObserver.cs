using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualOverrideObserver : IDisposable
{
    bool ManualOverrideOngoing { get; }
}

static class ManualOverrideObserver
{
    public static IManualOverrideObserver CreateSubscribed(ComponentEvents events)
    {
        var listener = new Listener(events);
        events.Subscribe<ManualOverrideStarted>(listener);
        events.Subscribe<ManualOverrideEnded>(listener);
        return listener;
    }

    private sealed class Listener(ComponentEvents events) : IManualOverrideObserver,
        IListener<ManualOverrideStarted>,
        IListener<ManualOverrideEnded>
    {
        public bool ManualOverrideOngoing { get; private set; }

        public void HandleEvent(ManualOverrideStarted @event)
        {
            ManualOverrideOngoing = true;
        }

        public void HandleEvent(ManualOverrideEnded @event)
        {
            ManualOverrideOngoing = false;
        }

        public void Dispose()
        {
            events.Unsubscribe<ManualOverrideStarted>(this);
            events.Unsubscribe<ManualOverrideEnded>(this);
        }
    }
}
