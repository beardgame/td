using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

abstract class ElementalPhenomenonScopeBase<TEffect> : IElementalPhenomenon.IScope<TEffect> where TEffect : IElementalEffect
{
    private readonly List<EffectWithExpiry> activeEffects = new();
    private bool hadEffectActive;

    protected IEnumerable<TEffect> ActiveEffects => activeEffects.Select(e => e.Effect);

    public void Adopt(TEffect effect, Instant now)
    {
        activeEffects.Add(new EffectWithExpiry(effect, now + effect.Duration));
    }

    public void ApplyTick(IComponentOwner target, Instant now)
    {
        activeEffects.RemoveAll(e => e.Expiry <= now);
        if (TryChooseEffect(out var effect))
        {
            ApplyEffectTick(target, effect);
            startEffectIfPreviouslyInactive(target);
        }
        else
        {
            endEffectIfPreviouslyActive(target);
        }
    }

    private void startEffectIfPreviouslyInactive(IComponentOwner target)
    {
        if (hadEffectActive) return;

        StartEffect(target);
        hadEffectActive = true;
    }

    private void endEffectIfPreviouslyActive(IComponentOwner target)
    {
        if (!hadEffectActive) return;

        EndEffect(target);
        hadEffectActive = false;
    }

    protected abstract bool TryChooseEffect(out TEffect effect);
    protected abstract void ApplyEffectTick(IComponentOwner target, TEffect effect);
    protected abstract void StartEffect(IComponentOwner target);
    protected abstract void EndEffect(IComponentOwner target);

    private readonly record struct EffectWithExpiry(TEffect Effect, Instant Expiry);
}
