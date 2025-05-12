using System;
using System.Collections.Generic;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Damage;

partial class HitPointsPool
{
    private readonly SortedList<Type, IDamageModifier> modifiers = new(ExplicitOrder.Create(DamageModifiers.Order));

    private TypedDamage modifyDamage(TypedDamage damage, out List<AdditionalHitEffect> additionalEffects)
    {
        additionalEffects = [];
        var preview = new DamagePreview(damage, additionalEffects);
        foreach (var effect in modifiers.Values)
        {
            effect.ModifyDamage(ref preview);
        }

        return new TypedDamage(preview.DamageAmount, preview.DamageType);
    }

    public void AddModifier(IDamageModifier modifier)
    {
        modifiers.Add(modifier.GetType(), modifier);
    }

    public void RemoveModifier(IDamageModifier modifier)
    {
        modifiers.Remove(modifier.GetType());
    }
}
