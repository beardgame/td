using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

abstract class ElementalPhenomenonScopeBase<TEffect> : IElementalPhenomenon.IScope<TEffect> where TEffect : IElementalEffect
{
    private readonly GameObject target;
    private readonly IStatusTracker? statusDisplay;
    private readonly List<EffectWithExpiry> activeEffects = [];
    private ActiveEffect? activeEffect;

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
            transitionToEffect(effect);
            ApplyEffectTick(target, effect);
        }
        else
        {
            endEffectIfPreviouslyActive();
        }

        if (activeEffect?.StatusIcon is not null)
        {
            updateStatusIconExpiry(activeEffect.StatusIcon);
        }
    }

    private void transitionToEffect(TEffect effect)
    {
        // null -> effect A
        if (activeEffect is null)
        {
            StartScope(target, out var createStatus);
            startEffect(effect, out var createOverrideStatus);
            createStatus = createOverrideStatus ?? createStatus;
            var receipt = createStatus is null ? null : reportStatus(createStatus);
            activeEffect = new ActiveEffect(effect, receipt);
            return;
        }

        // effect A -> effect A
        if (activeEffect.Effect.Equals(effect)) return;

        // effect A -> effect B
        var statusIcon = activeEffect.StatusIcon;
        EndEffect(target, effect);
        startEffect(effect, out var createNewStatus);
        if (createNewStatus is not null)
        {
            statusIcon?.DeleteImmediately();
            statusIcon = reportStatus(createNewStatus);
        }
        activeEffect = new ActiveEffect(effect, statusIcon);
    }

    private void startEffect(TEffect effect, out ElementalStatus? newStatus)
    {
        var ctx = new EffectStartContext();
        StartEffect(target, effect, ctx);
        newStatus = ctx.NewStatus;
    }

    private IStatusReceipt? reportStatus(ElementalStatus status)
    {
        var statusReceipt = statusDisplay?.AddStatus(
            new StatusSpec(StatusType.Negative, null),
            StatusAppearance.IconOnly(status.Sprite),
            null);
        return statusReceipt;
    }

    private void endEffectIfPreviouslyActive()
    {
        if (activeEffect is null) return;

        EndScope(target);
        activeEffect.StatusIcon?.DeleteImmediately();
        activeEffect = null;
    }

    private void updateStatusIconExpiry(IStatusReceipt statusIcon)
    {
        var latestExpiry = activeEffects.Select(e => e.Expiry).Max();
        statusIcon.SetExpiryTime(latestExpiry);
    }

    protected abstract bool TryChooseEffect(out TEffect effect);
    protected abstract void StartScope(GameObject target, out ElementalStatus? status);
    protected abstract void StartEffect(GameObject target, TEffect effect, EffectStartContext context);
    protected abstract void ApplyEffectTick(GameObject target, TEffect effect);
    protected abstract void EndEffect(GameObject target, TEffect effect);
    protected abstract void EndScope(GameObject target);

    private readonly record struct EffectWithExpiry(TEffect Effect, Instant Expiry);
    private sealed record ActiveEffect(TEffect Effect, IStatusReceipt? StatusIcon);

    protected class EffectStartContext
    {
        public ElementalStatus? NewStatus { get; private set; }

        public void ChangeStatus(ElementalStatus status)
        {
            NewStatus = status;
        }
    }
}
