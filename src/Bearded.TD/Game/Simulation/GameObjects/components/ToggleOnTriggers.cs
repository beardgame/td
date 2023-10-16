using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IToggle
{
    string Name { get; }
    bool Enabled { get; }
}

[Component("toggleOnTriggers")]
sealed class ToggleOnTriggers : Component<ToggleOnTriggers.IParameters>, IListener<ObjectDeleting>, IToggle
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        string Name { get; }
        bool StartDisabled { get; }
        ImmutableArray<ITrigger>? StartOn { get; }
        ImmutableArray<ITrigger>? StopOn { get; }
        ImmutableArray<ITrigger>? StartOnParent { get; }
        ImmutableArray<ITrigger>? StopOnParent { get; }
    }

    public string Name => Parameters.Name;
    public bool Enabled { get; private set; }
    private TriggerListener? startOnParentListener;
    private TriggerListener? stopOnParentListener;
    private readonly List<ITriggerSubscription> triggerSubscriptions = new();

    public ToggleOnTriggers(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        Enabled = !Parameters.StartDisabled;

        if(Parameters.StartOn is { } startOn)
            foreach (var trigger in startOn)
                triggerSubscriptions.Add(trigger.Subscribe(Events, turnOn));
        if (Parameters.StopOn is { } stopOn)
            foreach (var trigger in stopOn)
                triggerSubscriptions.Add(trigger.Subscribe(Events, turnOff));

        if (Owner.Parent is { } parent)
        {
            if (Parameters.StartOnParent is { Length: > 0 } startOnParent)
            {
                startOnParentListener = new TriggerListener(startOnParent, turnOn);
                parent.AddComponent(startOnParentListener);
            }
            if (Parameters.StopOnParent is { Length: > 0 } stopOnParent)
            {
                stopOnParentListener = new TriggerListener(stopOnParent, turnOff);
                parent.AddComponent(stopOnParentListener);
            }
        }
    }

    private void turnOff()
    {
        Enabled = false;
    }

    private void turnOn()
    {
        Enabled = true;
    }

    public override void OnRemoved()
    {
        cleanUp();
    }
    public void HandleEvent(ObjectDeleting @event)
    {
        cleanUp();
    }

    private void cleanUp()
    {
        foreach (var subscription in triggerSubscriptions)
        {
            subscription.Unsubscribe(Events);
        }

        if (Owner.Parent is { } parent)
        {
            if (startOnParentListener is { } startOnParent)
                parent.RemoveComponent(startOnParent);
            if (stopOnParentListener is { } stopOnParent)
                parent.RemoveComponent(stopOnParent);
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

