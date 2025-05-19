using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static class DisruptedAiming
{
    public readonly record struct Effect(TimeSpan Duration) : IElementalEffect<Effect>
    {
        public IElementalEffect<Effect>.IScope NewScope(GameObject target) => new Scope(target);
    }

    private sealed class Scope(GameObject target) : ElementalPhenomenonScopeBase<Effect>(target)
    {
        private IUpgradeReceipt? receipt;

        protected override Effect? ChooseEffect(ReadOnlySpan<EffectWithExpiry> activeEffects)
        {
            return activeEffects.IsEmpty ? null : activeEffects[0].Effect;
        }

        protected override void StartScope(ref EffectChangeResult statusChange)
        {
            var upgrade = Upgrade.FromEffects(
                new AddComponentFromDelegate(createComponent, UpgradePrerequisites.RequireTags("weapon"), false));
            if (!Target.CanApplyUpgrade(upgrade)) return;
            receipt = Target.ApplyUpgrade(upgrade);
            statusChange = new ElementalStatus("eye-disabled".ToStatusIconSpriteId());
        }

        private static TargetRandomTilesInRange createComponent() => new(
            new TargetRandomTilesInRangeParametersTemplate(
                retargetInterval: 0.3.S(), maxRetargetDistance: 3.U(), randomDrift: 4.UnitsPerSecond()));

        protected override void StartEffect(Effect effect, ref EffectChangeResult statusChange) { }

        protected override void ApplyEffectTick(Effect effect) { }

        protected override void EndEffect() { }

        protected override void EndScope()
        {
            receipt?.Rollback();
            receipt = null;
        }
    }
}
