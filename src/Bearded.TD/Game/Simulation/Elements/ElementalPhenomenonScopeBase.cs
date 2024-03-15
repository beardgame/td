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
    private readonly List<EffectWithExpiry> activeEffects = new();
    private IStatusDrawer? cachedStatusDrawer;
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
            ApplyEffectTick(target, effect);
            startEffectIfPreviouslyInactive();
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

    private void startEffectIfPreviouslyInactive()
    {
        if (activeEffect is not null) return;

        StartEffect(target);
        var statusReceipt = reportStatus();
        activeEffect = new ActiveEffect(statusReceipt);
    }

    private IStatusReceipt? reportStatus()
    {
        var elementalStatus = MakeStatus(target.Game.Meta.Blueprints);
        cachedStatusDrawer ??=
            IconStatusDrawer.FromSpriteBlueprint(
                target.Game,
                target.Game.Meta.Blueprints.Sprites[elementalStatus.Sprite.SpriteSet]
                    .GetSprite(elementalStatus.Sprite.Id));
        var statusReceipt = statusDisplay?.AddStatus(
            new StatusSpec(StatusType.Negative, StatusDrawSpec.StaticIcon(elementalStatus.Sprite), cachedStatusDrawer),
            null);
        return statusReceipt;
    }

    private void endEffectIfPreviouslyActive()
    {
        if (activeEffect is null) return;

        EndEffect(target);
        activeEffect.StatusIcon?.DeleteImmediately();
        activeEffect = null;
    }

    private void updateStatusIconExpiry(IStatusReceipt statusIcon)
    {
        var latestExpiry = activeEffects.Select(e => e.Expiry).Max();
        statusIcon.SetExpiryTime(latestExpiry);
    }

    protected abstract bool TryChooseEffect(out TEffect effect);
    protected abstract void ApplyEffectTick(GameObject target, TEffect effect);
    protected abstract void StartEffect(GameObject target);
    protected abstract void EndEffect(GameObject target);
    protected abstract ElementalStatus MakeStatus(Blueprints blueprints);

    private readonly record struct EffectWithExpiry(TEffect Effect, Instant Expiry);
    private sealed record ActiveEffect(IStatusReceipt? StatusIcon);
}
