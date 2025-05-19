using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static class ObscuredVision
{
    public readonly record struct Effect(double Factor, TimeSpan Duration) : IElementalEffect<Effect>
    {
        public IElementalEffect<Effect>.IScope NewScope(GameObject target) => new Scope(target);
    }

    private sealed class Scope(GameObject target) : ElementalPhenomenonScopeBase<Effect>(target)
    {
        private IUpgradeReceipt? receipt;

        protected override Effect? ChooseEffect(ReadOnlySpan<EffectWithExpiry> activeEffects)
        {
            return activeEffects.MinByOrDefault(e => e.Effect.Factor)?.Effect;
        }

        protected override void StartScope(ref EffectChangeResult statusChange)
        {
            statusChange = new ElementalStatus("eye-disabled".ToStatusIconSpriteId());

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
        }

        private static ModifyParameter createUpgradeEffect(Effect effect)
        {
            return new ModifyParameter(
                AttributeType.Range,
                Modification.MultiplyWith(effect.Factor),
                UpgradePrerequisites.Empty,
                false);
        }
    }
}
