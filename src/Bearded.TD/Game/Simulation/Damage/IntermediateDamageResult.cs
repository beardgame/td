namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct IntermediateDamageResult(
    TypedDamage ExactDamageDone,
    TypedDamage DamageOverflow,
    HitPoints DiscreteDamageDone,
    UntypedDamage ConsumedDamagePotential)
{
    public static IntermediateDamageResult PassThrough(TypedDamage damage) =>
        new(TypedDamage.Zero(damage.Type), damage, HitPoints.Zero, UntypedDamage.Zero);

    public static IntermediateDamageResult Blocked(TypedDamage damage) =>
        new(TypedDamage.Zero(damage.Type), TypedDamage.Zero(damage.Type), HitPoints.Zero, damage.Untyped());
}
