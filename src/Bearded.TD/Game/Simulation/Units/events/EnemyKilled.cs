using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Units;

readonly struct EnemyKilled : IGlobalEvent
{
    public EnemyUnit Unit { get; }
    public IDamageSource? DamageSource { get; }

    public EnemyKilled(EnemyUnit unit, IDamageSource? damageSource)
    {
        Unit = unit;
        DamageSource = damageSource;
    }
}