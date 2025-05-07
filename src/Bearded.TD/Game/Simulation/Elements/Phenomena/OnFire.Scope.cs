using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using static Bearded.TD.Constants.Game.Elements;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class OnFire
{
    private sealed class Scope(GameObject target) : ElementalPhenomenonScopeBase<Effect>(target)
    {
        private FireFlicker? fireFlicker;

        protected override bool TryChooseEffect(out Effect effect)
        {
            var effects = ActiveEffects.ToImmutableArray();
            if (effects.IsEmpty)
            {
                effect = default;
                return false;
            }

            effect = effects.MaxBy(e => e.DamagePerSecond.Amount.NumericValue);
            return true;
        }

        protected override void StartScope(GameObject target, out ElementalStatus? status)
        {
            fireFlicker = new FireFlicker();
            target.AddComponent(fireFlicker);
            status = new ElementalStatus("fire".ToStatusIconSpriteId());
        }

        protected override void StartEffect(GameObject target, Effect effect, EffectStartContext context) { }

        protected override void ApplyEffectTick(GameObject target, Effect effect)
        {
            var damage = effect.DamagePerSecond * TickDuration;
            DamageExecutor.FromDamageSource(effect.DamageSource)
                .TryDoDamage(target, damage.Typed(DamageType.Fire), Hit.FromSelf());
        }

        protected override void EndEffect(GameObject target, Effect effect) { }

        protected override void EndScope(GameObject target)
        {
            if (fireFlicker == null)
            {
                throw new InvalidOperationException("Cannot end effect that was not started.");
            }
            target.RemoveComponent(fireFlicker);
            fireFlicker = null;
        }
    }
}
