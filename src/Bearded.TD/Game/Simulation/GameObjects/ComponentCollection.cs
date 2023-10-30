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
    private readonly Queue<ComponentCollectionMutation> queuedMutations = new();

    public ComponentCollection(GameObject owner, ComponentEvents events)
    {
        this.owner = owner;
        this.events = events;
    }

    public void Add(IComponent component)
    {
        if (deferComponentCollectionMutations)
        {
            queuedMutations.Enqueue(ComponentCollectionMutation.Addition(component));
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
            queuedMutations.Enqueue(ComponentCollectionMutation.Removal(component));
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
            applyMutation(queuedMutations.Dequeue());
        }
    }

    public void ApplyMutations(IEnumerable<ComponentCollectionMutation> mutations)
    {
        foreach (var mutation in mutations)
        {
            applyMutation(mutation);
        }
    }

    private void applyMutation(ComponentCollectionMutation mutation)
    {
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
