using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IComponentOwner
{
    GameObject? Parent { get; }

    void AddComponent(IComponent component);
    void RemoveComponent(IComponent component);
    IEnumerable<T> GetComponents<T>();
}
