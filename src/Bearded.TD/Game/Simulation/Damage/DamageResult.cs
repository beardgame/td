namespace Bearded.TD.Game.Simulation.Damage;

readonly struct DamageResult
{
    public TypedDamage TypedDamage { get; }

    public DamageResult(TypedDamage typedDamage)
    {
        TypedDamage = typedDamage;
    }
}