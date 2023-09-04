namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct FinalDamageResult(
    TypedDamage TotalExactDamage, HitPoints TotalDiscreteDamage)
{
    public static FinalDamageResult None(DamageType damageType) => new(TypedDamage.Zero(damageType), HitPoints.Zero);
}
