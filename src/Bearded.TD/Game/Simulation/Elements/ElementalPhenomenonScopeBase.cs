using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

abstract class ElementalPhenomenonScopeBase<TEffect>
    : ElementalPhenomenonScopeBase, IElementalEffect<TEffect>.IScope
    where TEffect : struct, IElementalEffect<TEffect>, IEquatable<TEffect>
{
    private readonly IStatusTracker? statusDisplay;
    private readonly List<EffectWithExpiry> activeEffects = [];
    private ActiveEffect? activeEffect;

    protected GameObject Target { get; private set; }

    protected ElementalPhenomenonScopeBase(GameObject target)
    {
        Target = target;
        target.TryGetSingleComponent(out statusDisplay);
    }

    public void Adopt(TEffect effect, Instant now)
    {
        activeEffects.Add(new EffectWithExpiry(effect, now + effect.Duration));
    }

    public void ApplyTick(Instant now)
    {
        activeEffects.RemoveAll(now, static (t, e) => e.Expiry <= t);

        if (TryChooseEffect(CollectionsMarshal.AsSpan(activeEffects)) is { } effect)
        {
            transitionToEffect(effect);
            ApplyEffectTick(effect);
        }
        else
        {
            endEffectIfPreviouslyActive();
        }

        if (activeEffect?.StatusIcon is { } icon)
        {
            updateStatusIconExpiry(icon);
        }
    }

    private void transitionToEffect(TEffect effect)
    {
        // null -> effect A
        if (activeEffect is not { } current)
        {
            EffectChangeResult? statusChange = null;
            StartScope(effect, ref statusChange);
            var receipt = statusChange?.NewStatus is { } status ? reportStatus(status) : null;
            activeEffect = new ActiveEffect(effect, receipt);
            return;
        }

        // effect A -> effect A
        if (current.Effect.Equals(effect))
            return;

        // effect A -> effect B
        {
            EffectChangeResult? statusChange = null;
            ChangeActiveEffect(current.Effect, effect, ref statusChange);
            current.StatusIcon?.DeleteImmediately();
            var receipt = statusChange?.NewStatus is { } status ? reportStatus(status) : null;
            activeEffect = new ActiveEffect(effect, receipt);
        }
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
        if (activeEffect is not { } current) return;

        EndScope(current.Effect);
        current.StatusIcon?.DeleteImmediately();
        activeEffect = null;
    }

    private void updateStatusIconExpiry(IStatusReceipt statusIcon)
    {
        var latestExpiry = activeEffects.Max(static e => e.Expiry);
        statusIcon.SetExpiryTime(latestExpiry);
    }

    protected abstract TEffect? TryChooseEffect(ReadOnlySpan<EffectWithExpiry> activeEffects);
    protected abstract void StartScope(TEffect effect, ref EffectChangeResult? statusChange);
    protected abstract void ChangeActiveEffect(TEffect previousEffect, TEffect newEffect, ref EffectChangeResult? statusChange);
    protected abstract void ApplyEffectTick(TEffect effect);
    protected abstract void EndScope(TEffect effect);

    protected readonly record struct EffectWithExpiry(TEffect Effect, Instant Expiry);
    private readonly record struct ActiveEffect(TEffect Effect, IStatusReceipt? StatusIcon);

}

// Base class for non-generic members
abstract class ElementalPhenomenonScopeBase
{
    public readonly struct EffectChangeResult
    {
        public ElementalStatus? NewStatus { get; private init; }

        public static EffectChangeResult HideStatus => default;

        public static EffectChangeResult ShowStatus(ElementalStatus status) => new() { NewStatus = status };

        public static implicit operator EffectChangeResult(ElementalStatus newStatus) => ShowStatus(newStatus);
    }

    protected static EffectChangeResult HideStatus => EffectChangeResult.HideStatus;
    protected static EffectChangeResult ShowStatus(ElementalStatus status) => EffectChangeResult.ShowStatus(status);
}
