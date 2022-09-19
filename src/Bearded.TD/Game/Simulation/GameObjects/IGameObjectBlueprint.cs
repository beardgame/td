using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IGameObjectBlueprint : IBlueprint
{
    IEnumerable<IComponent> GetComponents();
    IEnumerable<IComponentFactory> GetFactories();
}
