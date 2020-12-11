using Bearded.TD.Game.Simulation.Units;

namespace Bearded.TD.Game.Simulation.Components.Weapons
{
    interface IEnemyUnitTargeter
    {
        EnemyUnit Target { get; }
    }
}
