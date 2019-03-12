using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Game.Units
{
    interface IUnitBlueprint : IBlueprint
    {
        string Name { get; }
        float Value { get; }
        Color Color { get; }
        
        IEnumerable<IComponent<EnemyUnit>> GetComponents();
    }
}
