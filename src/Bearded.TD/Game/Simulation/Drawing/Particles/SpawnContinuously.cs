using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Drawing.Particles.ParticleSpawning;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnContinuously")]
sealed class SpawnContinuously : ParticleUpdater<SpawnContinuously.IParameters>, IListener<ObjectDeleting>
{
    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        [Modifiable(1)]
        TimeSpan Interval { get; }
        float IntervalNoise { get; }

        bool StartDisabled { get; }

        ImmutableArray<ITrigger>? StartOn { get; }
        ImmutableArray<ITrigger>? StopOn { get; }
        ImmutableArray<ITrigger>? StartOnParent { get; }
        ImmutableArray<ITrigger>? StopOnParent { get; }
    }

    private Instant nextSpawn;
    private bool disabled;
    private TriggerListener? startOnParentListener;
    private TriggerListener? stopOnParentListener;
    private readonly List<ITriggerSubscription> triggerSubscriptions = new();

    public SpawnContinuously(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

        disabled = Parameters.StartDisabled;

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

        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        cleanUp();
    }

    public void HandleEvent(ObjectDeleting _)
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

    private void turnOff()
    {
        disabled = true;
    }

    private void turnOn()
    {
        disabled = false;
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (disabled)
            return;

        if (nextSpawn <= Owner.Game.Time)
        {
            spawn();
            nextSpawn = Owner.Game.Time + Parameters.Interval * Noise(Parameters.IntervalNoise);
        }
    }

    private void spawn()
    {
        Particles.CreateParticles(Parameters, Velocity3.Zero, Owner.Direction, Owner.Game.Time, Owner.Position);
    }
}

