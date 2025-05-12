using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static class Shocked
{
    public readonly record struct Effect(double MovementPenalty, TimeSpan Duration) : IElementalEffect<Effect>
    {
        public IElementalEffect<Effect>.IScope NewScope(GameObject target) => new Scope(target);
    }

    private sealed class Scope(GameObject target) : ElementalPhenomenonScopeBase<Effect>(target)
    {
        private IUpgradeReceipt? receipt;
        private LightningShocks? lightningShocks;

        protected override Effect? ChooseEffect(ReadOnlySpan<EffectWithExpiry> activeEffects)
        {
            return activeEffects.MaxByOrDefault(static e => e.Effect.MovementPenalty)?.Effect;
        }

        protected override void StartScope(ref EffectChangeResult statusChange)
        {
            lightningShocks = new LightningShocks();
            Target.AddComponent(lightningShocks);
            statusChange = new ElementalStatus("snail".ToStatusIconSpriteId());
        }

        protected override void StartEffect(Effect effect, ref EffectChangeResult statusChange)
        {
            var upgrade = Upgrade.FromEffects(createUpgradeEffect(effect));
            if (!Target.CanApplyUpgrade(upgrade)) return;
            receipt = Target.ApplyUpgrade(upgrade);
        }

        protected override void ApplyEffectTick(Effect effect) { }

        protected override void EndEffect()
        {
            receipt?.Rollback();
            receipt = null;
        }

        protected override void EndScope()
        {
            if (lightningShocks == null)
            {
                throw new InvalidOperationException("Cannot end effect that was not started.");
            }

            Target.RemoveComponent(lightningShocks);
            lightningShocks = null;
        }

        private static ModifyParameter createUpgradeEffect(Effect effect)
        {
            return new ModifyParameter(
                AttributeType.MovementSpeed,
                Modification.MultiplyWith(1 - effect.MovementPenalty),
                UpgradePrerequisites.Empty,
                false);
        }
    }
}
