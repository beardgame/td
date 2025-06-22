namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct FinalDamageResult(
    TypedDamage TotalExactDamage,
    HitPoints TotalDiscreteDamage,
    TypedDamage AttemptedDamage,
    UntypedDamage ConsumedDamagePotential)
{
    public static FinalDamageResult None(DamageType damageType) =>
        new(TypedDamage.Zero(damageType), HitPoints.Zero, TypedDamage.Zero(damageType), UntypedDamage.Zero);
}
