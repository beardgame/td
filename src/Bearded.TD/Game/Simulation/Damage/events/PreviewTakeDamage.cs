using Bearded.TD.Game.Simulation.Components;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct PreviewTakeDamage : IComponentPreviewEvent
{
    public DamageInfo DamageInfo { get; }
    public HitPoints? DamageCap { get; }

    public PreviewTakeDamage(DamageInfo damageInfo) : this(damageInfo, null) {}

    private PreviewTakeDamage(DamageInfo damageInfo, HitPoints? damageCap)
    {
        Argument.Satisfies(damageCap == null || damageCap >= HitPoints.Zero);

        DamageInfo = damageInfo;
        DamageCap = damageCap;
    }

    public PreviewTakeDamage CappedAt(HitPoints damageCap)
    {
        return new PreviewTakeDamage(
            DamageInfo,
            DamageCap is { } existingDamageCap && damageCap > existingDamageCap
                ? existingDamageCap
                : damageCap);
    }
}