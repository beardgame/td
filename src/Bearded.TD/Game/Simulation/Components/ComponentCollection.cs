using System;
using System.Collections.Generic;
using System.Linq;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components;

sealed class ComponentCollection<TOwner>
{
    private readonly TOwner owner;
    private readonly ComponentEvents events;
    private readonly List<IComponent<TOwner>> components = new();

    private bool deferComponentCollectionMutations;
    private readonly Queue<CollectionMutation> queuedMutations = new();

    public ComponentCollection(TOwner owner, ComponentEvents events)
    {
        this.owner = owner;
        this.events = events;
    }

    public void Add(IEnumerable<IComponent<TOwner>> newComponents)
    {
        foreach (var component in newComponents)
        {
            Add(component);
        }
    }

    public void Add(IComponent<TOwner> component)
    {
        if (deferComponentCollectionMutations)
        {
            queuedMutations.Enqueue(CollectionMutation.Addition(component));
            return;
        }
        components.Add(component);
        component.OnAdded(owner, events);
        events.Send(new ComponentAdded(component));
    }

    public void Remove(IComponent<TOwner> component)
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
        public static CollectionMutation Addition(IComponent<TOwner> component) =>
            new(CollectionMutationType.Addition, component);

        public static CollectionMutation Removal(IComponent<TOwner> component) =>
            new(CollectionMutationType.Removal, component);

        public CollectionMutationType Type { get; }
        public IComponent<TOwner> Component { get; }

        private CollectionMutation(CollectionMutationType type, IComponent<TOwner> component)
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
