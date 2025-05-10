using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class Stunned
{
    private sealed class Scope : ElementalPhenomenonScopeBase<Effect>
    {
        private IBreakageReceipt? receipt;
        private LightningShocks? sparks;

        public Scope(GameObject target) : base(target) { }

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

        protected override void StartScope(GameObject target, out ElementalStatus? status)
        {
            if (receipt != null || sparks != null)
            {
                throw new InvalidOperationException("Cannot start effect that was already started.");
            }

            if (!target.TryGetSingleComponent<IBreakageHandler>(out var breakageHandler))
            {
                status = null;
                return;
            }

            receipt = breakageHandler.BreakObject();
            sparks = new LightningShocks();
            target.AddComponent(sparks);

            status = new ElementalStatus("unstable-orb".ToStatusIconSpriteId());
        }

        protected override void StartEffect(GameObject target, Effect effect, EffectStartContext context) { }

        protected override void ApplyEffectTick(GameObject target, Effect effect) {}

        protected override void EndEffect() { }

        protected override void EndScope(GameObject target)
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
