using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Damage;

abstract class DamagePreview(DamageType type, UntypedDamage damage)
{
    private readonly List<AdditionalHitEffect> additionalEffects = [];

    public DamageType Type => type;
    public UntypedDamage UnmodifiedDamage { get; } = damage;

    public Resistance? DamageResistance { get; private set; }
    public IReadOnlyList<AdditionalHitEffect> AdditionalEffects => additionalEffects;

    public void Resist(Resistance r)
    {
        DamageResistance = Resistance.Max(r, DamageResistance ?? Resistance.Zero);
    }

    public void AddAdditionalEffect(AdditionalHitEffect effect)
    {
        additionalEffects.Add(effect);
    }
}
