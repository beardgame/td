using Bearded.TD.Game.Simulation.GameObjects;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct PreviewTakeDamage : IComponentPreviewEvent
{
    public TypedDamage TypedDamage { get; }
    public HitPoints? DamageCap { get; }

    public PreviewTakeDamage(TypedDamage typedDamage) : this(typedDamage, null) {}

    private PreviewTakeDamage(TypedDamage typedDamage, HitPoints? damageCap)
    {
        Argument.Satisfies(damageCap == null || damageCap >= HitPoints.Zero);

        TypedDamage = typedDamage;
        DamageCap = damageCap;
    }

    public PreviewTakeDamage CappedAt(HitPoints damageCap)
    {
        return new PreviewTakeDamage(
            TypedDamage,
            DamageCap is { } existingDamageCap && damageCap > existingDamageCap
                ? existingDamageCap
                : damageCap);
    }
}
