using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static class LowerWeaponRange
{
    public readonly record struct Effect(double Factor, TimeSpan Duration) : IElementalEffect
    {
        IElementalPhenomenon IElementalEffect.Phenomenon => phenomenon;
    }

    private static readonly IElementalPhenomenon phenomenon = new Phenomenon();

    private sealed class Phenomenon : IElementalPhenomenon
    {
        public Type EffectType => typeof(Effect);

        public IElementalPhenomenon.IScope NewScope(GameObject target) => new Scope(target);
    }

    private sealed class Scope(GameObject target) : ElementalPhenomenonScopeBase<Effect>(target)
    {
        private IUpgradeReceipt? receipt;

        protected override bool TryChooseEffect(out Effect effect)
        {
            var effects = ActiveEffects.ToImmutableArray();
            if (effects.IsEmpty)
            {
                effect = default;
                return false;
            }

            effect = effects.MinBy(e => e.Factor);
            return true;
        }

        protected override void BeforeEffectStart(GameObject target, out ElementalStatus? status)
        {
            status = new ElementalStatus("eye-disabled".ToStatusIconSpriteId());
        }

        protected override void StartActiveEffect(GameObject target, Effect effect, EffectStartContext context)
        {
            var upgrade = Upgrade.FromEffects(createUpgradeEffect(effect));
            if (!target.CanApplyUpgrade(upgrade)) return;
            receipt = target.ApplyUpgrade(upgrade);
        }

        protected override void ApplyEffectTick(GameObject target, Effect effect) { }

        protected override void EndActiveEffect(GameObject target, Effect effect)
        {
            receipt?.Rollback();
            receipt = null;
        }

        protected override void AfterEffectEnd(GameObject target) { }

        private static IUpgradeEffect createUpgradeEffect(Effect effect)
        {
            return new ModifyParameter(
                AttributeType.Range,
                Modification.MultiplyWith(effect.Factor),
                UpgradePrerequisites.Empty,
                false);
        }
    }
}
