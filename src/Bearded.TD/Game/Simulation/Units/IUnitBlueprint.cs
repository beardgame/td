using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Units;

interface IUnitBlueprint : IBlueprint
{
    string Name { get; }
    float Value { get; }

    IEnumerable<IComponent<EnemyUnit>> GetComponents();
}