using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IComponentOwner
{
    IComponentOwner? Parent { get; }

    void AddComponent(IComponent component);
    void RemoveComponent(IComponent component);
    IEnumerable<T> GetComponents<T>();
}
