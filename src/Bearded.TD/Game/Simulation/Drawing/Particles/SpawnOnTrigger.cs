using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Drawing.Particles.ParticleSpawning;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnOnTrigger")]
sealed class SpawnOnTrigger : ParticleUpdater<SpawnOnTrigger.IParameters>, IListener<ObjectDeleting>
{
    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        TimeSpan Delay { get; }
        float DelayNoise { get; }

        ImmutableArray<ITrigger>? Triggers { get; }
        ImmutableArray<ITrigger>? ParentTriggers { get; }

        int ParentHierarchyDepth { get; }
    }

    private readonly Queue<Instant> queue = new();

    private GameObject? parent;
    private TriggerListener? parentListener;
    private readonly List<ITriggerSubscription> triggerSubscriptions = new();

    public SpawnOnTrigger(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

        if (Parameters.Triggers is { } triggers)
            foreach (var trigger in triggers)
                triggerSubscriptions.Add(trigger.Subscribe(Events, onTrigger));

        if (Parameters.ParentTriggers is { Length: > 0 } parentTriggers)
        {
            parent = getCorrectAncestor();

            if (parent != null)
            {
                parentListener = new TriggerListener(parentTriggers, onTrigger);
                parent.AddComponent(parentListener);
            }
        }

        Events.Subscribe(this);
    }

    private GameObject? getCorrectAncestor()
    {
        var parent = Owner.Parent;
        for (var i = 0; i < Parameters.ParentHierarchyDepth; i++)
        {
            if (parent is null)
                break;
            parent = parent.Parent;
        }
        return parent;
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

        if (parent != null && parentListener is { } listener)
        {
            parent.RemoveComponent(listener);
        }
    }

    private void onTrigger()
    {
        if (Parameters.Delay > 0.S())
            queue.Enqueue(Owner.Game.Time + Parameters.Delay * Noise(Parameters.DelayNoise));
        else
            spawn();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        while(queue.Count > 0 && queue.Peek() < Owner.Game.Time)
        {
            spawn();
            queue.Dequeue();
        }
    }

    private void spawn()
    {
        Particles.CreateParticles(Parameters, Velocity3.Zero, Owner.Direction, Owner.Game.Time, Owner.Position);
    }
}

