using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Units
{
    interface IUnitBlueprint : IBlueprint
    {
        string Name { get; }
        float Value { get; }
        Color Color { get; }
        
        IEnumerable<IComponent<EnemyUnit>> GetComponents();
    }
}
