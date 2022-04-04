using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Units;

readonly struct EnemyKilled : IGlobalEvent
{
    public GameObject Unit { get; }
    public IDamageSource? DamageSource { get; }

    public EnemyKilled(GameObject unit, IDamageSource? damageSource)
    {
        Unit = unit;
        DamageSource = damageSource;
    }
}
