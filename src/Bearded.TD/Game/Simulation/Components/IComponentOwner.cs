using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Components;

interface IComponentOwner
{
    IComponentOwner? Parent { get; }

    IEnumerable<T> GetComponents<T>();
}

interface IComponentOwner<out T> : IComponentOwner where T : IComponentOwner<T>
{
    void AddComponent(IComponent component);
    void RemoveComponent(IComponent component);
    new IEnumerable<TComponent> GetComponents<TComponent>() where TComponent : IComponent;
}
