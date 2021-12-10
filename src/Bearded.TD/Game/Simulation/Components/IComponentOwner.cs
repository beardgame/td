using System.Collections.Generic;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Components;

interface IComponentOwner
{
    IComponentOwner? Parent { get; }

    IEnumerable<T> GetComponents<T>();
}

interface IComponentOwner<out T> : IComponentOwner where T : IComponentOwner<T>
{
    void AddComponent(IComponent<T> component);
    void RemoveComponent(IComponent<T> component);
    new IEnumerable<TComponent> GetComponents<TComponent>() where TComponent : IComponent<T>;
}