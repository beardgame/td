using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.GameState.Components;

namespace Bearded.TD.Game.GameState.Units
{
    interface IUnitBlueprint : IBlueprint
    {
        string Name { get; }
        float Value { get; }
        Color Color { get; }
        
        IEnumerable<IComponent<EnemyUnit>> GetComponents();
    }
}
