using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Units;

readonly struct EnemyKilled : IGlobalEvent
{
    public ComponentGameObject Unit { get; }
    public IDamageSource? DamageSource { get; }

    public EnemyKilled(ComponentGameObject unit, IDamageSource? damageSource)
    {
        Unit = unit;
        DamageSource = damageSource;
    }
}
