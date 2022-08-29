using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class Shocked
{
    private sealed class Scope : ElementalPhenomenonScopeBase<Effect>
    {
        private IUpgradeReceipt? receipt;
        private LightningShocks? lightningShocks;

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

        protected override void ApplyEffectTick(GameObject target, Effect effect)
        {
            receipt?.Rollback();
            var upgrade = Upgrade.FromEffects(createUpgradeEffect(target.Game, effect));
            if (!target.CanApplyUpgrade(upgrade)) return;
            receipt = target.ApplyUpgrade(upgrade);
        }

        private static IUpgradeEffect createUpgradeEffect(GameState gameState, Effect effect)
        {
            return new ParameterModifiableWithId(
                AttributeType.MovementSpeed,
                new ModificationWithId(
                    gameState.GamePlayIds.GetNext<Modification>(),
                    Modification.MultiplyWith(1 - effect.MovementPenalty)),
                UpgradePrerequisites.Empty);
        }

        protected override void StartEffect(GameObject target)
        {
            lightningShocks = new LightningShocks();
            target.AddComponent(lightningShocks);
        }

        protected override void EndEffect(GameObject target)
        {
            if (receipt == null || lightningShocks == null)
            {
                throw new InvalidOperationException("Cannot end effect that was not started.");
            }
            receipt.Rollback();
            target.RemoveComponent(lightningShocks);
            receipt = null;
            lightningShocks = null;
        }
    }
}
