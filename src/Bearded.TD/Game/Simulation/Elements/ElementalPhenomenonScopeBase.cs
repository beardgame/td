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

        if (ChooseEffect(CollectionsMarshal.AsSpan(activeEffects)) is { } effect)
        {
            transitionToEffect(effect);
            ApplyEffectTick(effect);
        }
        else
        {
            endEffectIfPreviouslyActive();
        }

        if (activeEffect?.StatusReceipt is { } icon)
        {
            updateStatusIconExpiry(icon);
        }
    }

    private void transitionToEffect(TEffect effect)
    {
        // null -> effect A
        if (activeEffect is not { } current)
        {
            var statusChange = EffectChangeResult.NoChange;
            StartScope(ref statusChange);
            StartEffect(effect, ref statusChange);

            var receipt = statusChange.Type == EffectChangeType.ShowStatusIcon
                ? reportStatus(statusChange.NewStatus!.Value)
                : null;

            activeEffect = new ActiveEffect(effect, receipt);
            return;
        }

        // effect A -> effect A
        if (current.Effect.Equals(effect))
            return;

        // effect A -> effect B
        {
            var statusChange = EffectChangeResult.NoChange;
            EndEffect();
            StartEffect(effect, ref statusChange);

            var receipt = current.StatusReceipt;

            if (statusChange.Type == EffectChangeType.ShowStatusIcon)
            {
                receipt?.DeleteImmediately();
                receipt = reportStatus(statusChange.NewStatus!.Value);
            }

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

        EndEffect();
        EndScope();
        current.StatusReceipt?.DeleteImmediately();
        activeEffect = null;
    }

    private void updateStatusIconExpiry(IStatusReceipt statusIcon)
    {
        var latestExpiry = activeEffects.Max(static e => e.Expiry);
        statusIcon.SetExpiryTime(latestExpiry);
    }

    protected abstract TEffect? ChooseEffect(ReadOnlySpan<EffectWithExpiry> activeEffects);
    protected abstract void StartScope(ref EffectChangeResult statusChange);
    protected abstract void StartEffect(TEffect effect, ref EffectChangeResult statusChange);
    protected abstract void ApplyEffectTick(TEffect effect);
    protected abstract void EndEffect();
    protected abstract void EndScope();

    protected readonly record struct EffectWithExpiry(TEffect Effect, Instant Expiry);
    private readonly record struct ActiveEffect(TEffect Effect, IStatusReceipt? StatusReceipt);

}

// Base class for non-generic members
abstract class ElementalPhenomenonScopeBase
{
    protected enum EffectChangeType
    {
        Retain = 0,
        ShowStatusIcon = 1,
    }

    protected readonly struct EffectChangeResult
    {
        public EffectChangeType Type { get; private init; }

        public ElementalStatus? NewStatus { get; private init; }

        public static EffectChangeResult NoChange => default;

        public static EffectChangeResult ShowStatus(ElementalStatus status) => new()
        {
            Type = EffectChangeType.ShowStatusIcon,
            NewStatus = status,
        };

        public static implicit operator EffectChangeResult(ElementalStatus newStatus) => ShowStatus(newStatus);
    }
}
