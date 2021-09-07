using Bearded.TD.Game.Simulation.Units;

namespace Bearded.TD.Game.Simulation.Weapons
{
    interface IEnemyUnitTargeter
    {
        EnemyUnit Target { get; }
    }
}
