using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class Stunned
{
    private sealed class Scope : ElementalPhenomenonScopeBase<Effect>
    {
        private ISabotageReceipt? receipt;
        private LightningShocks? sparks;

        protected override bool TryChooseEffect(out Effect effect)
        {
            var effects = ActiveEffects.ToImmutableArray();
            if (effects.IsEmpty)
            {
                effect = default;
                return false;
            }

            effect = effects.MaxBy(e => e.Duration);
            return true;
        }

        protected override void ApplyEffectTick(GameObject target, Effect effect) {}

        protected override void StartEffect(GameObject target)
        {
            if (receipt != null || sparks != null)
            {
                throw new InvalidOperationException("Cannot start effect that was already started.");
            }

            if (!target.TryGetSingleComponent<ISabotageHandler>(out var sabotageHandler))
            {
                return;
            }

            receipt = sabotageHandler.SabotageObject();
            sparks = new LightningShocks();
            target.AddComponent(sparks);
        }

        protected override void EndEffect(GameObject target)
        {
            receipt?.Repair();
            receipt = null;

            if (sparks != null)
            {
                target.RemoveComponent(sparks);
                sparks = null;
            }
        }
    }
}
