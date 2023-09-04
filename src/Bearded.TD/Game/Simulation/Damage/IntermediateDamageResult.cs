namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct IntermediateDamageResult(
    TypedDamage ExactDamageDone,
    TypedDamage DamageOverflow,
    HitPoints DiscreteDamageDone)
{
    public static IntermediateDamageResult PassThrough(TypedDamage damage) =>
        new(TypedDamage.Zero(damage.Type), damage, HitPoints.Zero);

    public static IntermediateDamageResult Blocked(TypedDamage damage) =>
        new(TypedDamage.Zero(damage.Type), TypedDamage.Zero(damage.Type), HitPoints.Zero);

    public FinalDamageResult AsFinal() => new(ExactDamageDone, DiscreteDamageDone);
}
