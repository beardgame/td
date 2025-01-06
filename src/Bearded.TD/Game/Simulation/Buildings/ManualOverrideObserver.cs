using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IManualOverrideObserver : IDisposable
{
    bool ManualOverrideOngoing { get; }

    event VoidEventHandler? OverrideStarted;
    event VoidEventHandler? OverrideEnded;
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
        public event VoidEventHandler? OverrideStarted;
        public event VoidEventHandler? OverrideEnded;

        public void HandleEvent(ManualOverrideStarted @event)
        {
            ManualOverrideOngoing = true;
            OverrideStarted?.Invoke();
        }

        public void HandleEvent(ManualOverrideEnded @event)
        {
            ManualOverrideOngoing = false;
            OverrideEnded?.Invoke();
        }

        public void Dispose()
        {
            events.Unsubscribe<ManualOverrideStarted>(this);
            events.Unsubscribe<ManualOverrideEnded>(this);
            OverrideStarted = null;
            OverrideEnded = null;
        }
    }
}
