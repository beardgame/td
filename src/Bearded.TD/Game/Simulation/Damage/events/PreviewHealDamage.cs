using Bearded.TD.Game.Simulation.GameObjects;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct PreviewHealDamage : IComponentPreviewEvent
{
    public HealInfo HealInfo { get; }
    public HitPoints? HealCap { get; }

    public PreviewHealDamage(HealInfo healInfo) : this(healInfo, null) {}

    private PreviewHealDamage(HealInfo healInfo, HitPoints? healCap)
    {
        Argument.Satisfies(healCap == null || healCap >= HitPoints.Zero);

        HealInfo = healInfo;
        HealCap = healCap;
    }

    public PreviewHealDamage CappedAt(HitPoints healCap)
    {
        return new PreviewHealDamage(
            HealInfo,
            HealCap is { } existingHealCap && healCap > existingHealCap
                ? existingHealCap
                : healCap);
    }
}
