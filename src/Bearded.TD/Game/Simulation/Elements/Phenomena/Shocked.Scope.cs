using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class Shocked
{
    private sealed class Scope : ElementalPhenomenonScopeBase<Effect>
    {
        private IUpgradeReceipt? receipt;
        private LightningShocks? lightningShocks;

        public Scope(GameObject target) : base(target) { }

        protected override bool TryChooseEffect(out Effect effect)
        {
            var effects = ActiveEffects.ToImmutableArray();
            if (effects.IsEmpty)
            {
                effect = default;
                return false;
            }

            effect = effects.MaxBy(e => e.MovementPenalty);
            return true;
        }

        private static IUpgradeEffect createUpgradeEffect(Effect effect)
        {
            return new ModifyParameter(
                AttributeType.MovementSpeed,
                Modification.MultiplyWith(1 - effect.MovementPenalty),
                UpgradePrerequisites.Empty,
                false);
        }

        protected override void StartScope(GameObject target, out ElementalStatus? status)
        {
            lightningShocks = new LightningShocks();
            target.AddComponent(lightningShocks);
            status = new ElementalStatus("snail".ToStatusIconSpriteId());
        }

        protected override void StartEffect(GameObject target, Effect effect, EffectStartContext context)
        {
            var upgrade = Upgrade.FromEffects(createUpgradeEffect(effect));
            if (!target.CanApplyUpgrade(upgrade)) return;
            receipt = target.ApplyUpgrade(upgrade);
        }

        protected override void ApplyEffectTick(GameObject target, Effect effect) { }

        protected override void EndEffect(GameObject target, Effect effect)
        {
            receipt?.Rollback();
            receipt = null;
        }

        protected override void EndScope(GameObject target)
        {
            if (lightningShocks == null)
            {
                throw new InvalidOperationException("Cannot end effect that was not started.");
            }

            target.RemoveComponent(lightningShocks);
            lightningShocks = null;
        }
    }
}
