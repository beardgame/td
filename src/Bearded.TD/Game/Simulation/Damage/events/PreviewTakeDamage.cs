using Bearded.TD.Game.Simulation.GameObjects;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct PreviewTakeDamage(
        TypedDamage TypedDamage, HitPoints? DamageCap = null, Resistance? Resistance = null)
    : IComponentPreviewEvent
{
    public PreviewTakeDamage CappedAt(HitPoints damageCap)
    {
        Argument.Satisfies(damageCap >= HitPoints.Zero);
        if (DamageCap is { } existingDamageCap && existingDamageCap >= damageCap)
        {
            return this;
        }
        return this with
        {
            DamageCap = damageCap
        };
    }

    public PreviewTakeDamage ResistedWith(Resistance resistance)
    {
        if (Resistance is { } existingResistance && existingResistance >= resistance)
        {
            return this;
        }
        return this with
        {
            Resistance = resistance
        };
    }
}
