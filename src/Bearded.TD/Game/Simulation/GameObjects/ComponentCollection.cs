using System;
using System.Collections.Generic;
using System.Linq;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class ComponentCollection
{
    private readonly GameObject owner;
    private readonly ComponentEvents events;
    private readonly List<IComponent> components = new();

    private bool isActivated;
    private bool deferComponentCollectionMutations;
    private readonly Queue<CollectionMutation> queuedMutations = new();

    public ComponentCollection(GameObject owner, ComponentEvents events)
    {
        this.owner = owner;
        this.events = events;
    }

    public void Add(IComponent component)
    {
        if (deferComponentCollectionMutations)
        {
            queuedMutations.Enqueue(CollectionMutation.Addition(component));
            return;
        }
        components.Add(component);
        component.OnAdded(owner, events);
        events.Send(new ComponentAdded(component));
        if (isActivated)
        {
            component.Activate();
        }
    }

    public void Activate()
    {
        deferComponentCollectionMutations = true;
        foreach (var component in components)
        {
            component.Activate();
        }
        deferComponentCollectionMutations = false;
        isActivated = true;
        applyDeferredMutations();
    }

    public void Remove(IComponent component)
    {
        if (deferComponentCollectionMutations)
        {
            queuedMutations.Enqueue(CollectionMutation.Removal(component));
            return;
        }
        components.Remove(component);
        component.OnRemoved();
        events.Send(new ComponentRemoved(component));
    }

    public IEnumerable<T> Get<T>()
    {
        return components.OfType<T>();
    }

    public IEnumerable<IComponent> Find(string key)
    {
        return components.Where(c => c.Keys.Contains(key));
    }

    public void Update(TimeSpan elapsedTime)
    {
        deferComponentCollectionMutations = true;
        foreach (var component in components)
        {
            component.Update(elapsedTime);
        }
        deferComponentCollectionMutations = false;
        applyDeferredMutations();
    }

    private void applyDeferredMutations()
    {
        while (queuedMutations.Count > 0)
        {
            var mutation = queuedMutations.Dequeue();
            switch (mutation.Type)
            {
                case CollectionMutationType.Addition:
                    Add(mutation.Component);
                    break;
                case CollectionMutationType.Removal:
                    Remove(mutation.Component);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private readonly struct CollectionMutation
    {
        public static CollectionMutation Addition(IComponent component) =>
            new(CollectionMutationType.Addition, component);

        public static CollectionMutation Removal(IComponent component) =>
            new(CollectionMutationType.Removal, component);

        public CollectionMutationType Type { get; }
        public IComponent Component { get; }

        private CollectionMutation(CollectionMutationType type, IComponent component)
        {
            Type = type;
            Component = component;
        }
    }

    private enum CollectionMutationType
    {
        Addition,
        Removal
    }
}
