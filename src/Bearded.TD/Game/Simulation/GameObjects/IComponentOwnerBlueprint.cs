using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IComponentOwnerBlueprint : IBlueprint
{
    IEnumerable<IComponent> GetComponents();
    IEnumerable<IComponentFactory> GetFactories();
}
