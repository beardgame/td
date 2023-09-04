using Bearded.TD.Game.Simulation.GameObjects;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct PreviewTakeDamage(TypedDamage TypedDamage, Resistance? Resistance = null)
    : IComponentPreviewEvent
{
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
