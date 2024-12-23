using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IGameObjectBlueprint : IBlueprint
{
    IEnumerable<IComponent> GetComponents();
}
