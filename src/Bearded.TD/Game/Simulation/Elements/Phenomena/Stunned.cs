using System;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static class Stunned
{
    public readonly record struct Effect(TimeSpan Duration) : IElementalEffect<Effect>
    {
        public IElementalEffect<Effect>.IScope NewScope(GameObject target) => new Scope(target);
    }

    private sealed class Scope(GameObject target) : ElementalPhenomenonScopeBase<Effect>(target)
    {
        private IBreakageReceipt? receipt;
        private LightningShocks? sparks;

        protected override Effect? ChooseEffect(ReadOnlySpan<EffectWithExpiry> activeEffects)
        {
            return activeEffects.MaxByOrDefault(static e => e.Effect.Duration)?.Effect;
        }

        protected override void StartScope(ref EffectChangeResult statusChange)
        {
            if (!Target.TryGetSingleComponent<IBreakageHandler>(out var breakageHandler))
            {
                return;
            }

            receipt = breakageHandler.BreakObject();
            sparks = new LightningShocks();
            Target.AddComponent(sparks);

            statusChange = new ElementalStatus("unstable-orb".ToStatusIconSpriteId());
        }

        protected override void StartEffect(Effect effect, ref EffectChangeResult statusChange) { }

        protected override void ApplyEffectTick(Effect effect) { }

        protected override void EndEffect() { }

        protected override void EndScope()
        {
            receipt?.Repair();
            receipt = null;

            if (sparks != null)
            {
                Target.RemoveComponent(sparks);
                sparks = null;
            }
        }
    }
}
