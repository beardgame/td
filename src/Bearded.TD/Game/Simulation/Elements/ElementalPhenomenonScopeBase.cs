using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

abstract class ElementalPhenomenonScopeBase<TEffect> : IElementalPhenomenon.IScope<TEffect> where TEffect : IElementalEffect
{
    private readonly GameObject target;
    private readonly List<EffectWithExpiry> activeEffects = new();
    private bool hadEffectActive;

    protected IEnumerable<TEffect> ActiveEffects => activeEffects.Select(e => e.Effect);

    protected ElementalPhenomenonScopeBase(GameObject target)
    {
        this.target = target;
        target.TryGetSingleComponent(out statusDisplay);
    }

    public void Adopt(TEffect effect, Instant now)
    {
        activeEffects.Add(new EffectWithExpiry(effect, now + effect.Duration));
    }

    public void ApplyTick(Instant now)
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

    private void startEffectIfPreviouslyInactive()
    {
        if (hadEffectActive) return;

        StartEffect(target);
        hadEffectActive = true;
    }

    {
        if (!hadEffectActive) return;
    private void endEffectIfPreviouslyActive()

        EndEffect(target);
        hadEffectActive = false;
    }

    protected abstract bool TryChooseEffect(out TEffect effect);
    protected abstract void ApplyEffectTick(GameObject target, TEffect effect);
    protected abstract void StartEffect(GameObject target);
    protected abstract void EndEffect(GameObject target);

    private readonly record struct EffectWithExpiry(TEffect Effect, Instant Expiry);
}
