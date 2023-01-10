
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DivineIntervention : IDamageSource
{
    public static DivineIntervention DamageSource { get; } = new();

    private DivineIntervention() {}

    public void AttributeDamage(DamageResult result, GameObject damagedObject) {}

    public void AttributeKill() {}
}
