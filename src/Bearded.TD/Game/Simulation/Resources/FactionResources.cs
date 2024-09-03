using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Resources;

[FactionBehavior("resources")]
sealed class FactionResources : FactionBehavior
{
    private readonly Dictionary<Type, object> stores = new();

    protected override void Execute() {}

    public Resource<T> GetCurrent<T>()
        where T : IResourceType
    {
        return getStore<T>().Current;
    }

    public void ProvideResources<T>(Resource<T> resource)
        where T : IResourceType
    {
        var store = getStore<T>();
        store.Provide(resource);
        Events.Send(new ResourcesProvided<T>(this, resource));
        Events.Send(new ResourcesChanged<T>(this, store.Current));
    }

    public void ConsumeResources<T>(Resource<T> resource)
        where T : IResourceType
    {
        var store = getStore<T>();
        store.Consume(resource);
        Events.Send(new ResourcesConsumed<T>(this, resource));
        Events.Send(new ResourcesChanged<T>(this, store.Current));
    }

    private Store<T> getStore<T>()
        where T : IResourceType
    {
        if (stores.TryGetValue(typeof(T), out var r))
        {
            return (Store<T>) r;
        }
        var store = new Store<T>();
        stores[typeof(T)] = store;
        return store;
    }

    private sealed class Store<T>
        where T : IResourceType
    {
        public Resource<T> Current { get; private set; }

        public void Provide(Resource<T> amount)
        {
            if (amount < Resource<T>.Zero)
            {
                throw new InvalidOperationException("Cannot provide negative resources.");
            }
            Current += amount;
        }

        public void Consume(Resource<T> amount)
        {
            if (amount < Resource<T>.Zero)
            {
                throw new InvalidOperationException("Cannot consume negative resources.");
            }
            if (Current < amount)
            {
                throw new InvalidOperationException("Not enough resources available.");
            }
            Current -= amount;
        }
    }
}
