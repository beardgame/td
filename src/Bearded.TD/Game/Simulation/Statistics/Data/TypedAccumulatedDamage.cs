using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Statistics.Data;

sealed record TypedAccumulatedDamage(DamageType Type, AccumulatedDamage AccumulatedDamage)
{
    public UntypedDamage DamageDone => AccumulatedDamage.DamageDone;
    public UntypedDamage AttemptedDamage => AccumulatedDamage.AttemptedDamage;
    public double Efficiency => AccumulatedDamage.Efficiency;
}
