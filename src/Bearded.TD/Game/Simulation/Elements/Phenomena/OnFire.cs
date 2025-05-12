using System;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Constants.Game.Elements;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static class OnFire
{
    public readonly record struct Effect(
        UntypedDamagePerSecond DamagePerSecond, IDamageSource? DamageSource, TimeSpan Duration) : IElementalEffect<Effect>
    {
        public IElementalEffect<Effect>.IScope NewScope(GameObject target) => new Scope(target);
    }

    private sealed class Scope(GameObject target) : ElementalPhenomenonScopeBase<Effect>(target)
    {
        private FireFlicker? fireFlicker;

        protected override Effect? ChooseEffect(ReadOnlySpan<EffectWithExpiry> activeEffects)
        {
            return activeEffects.MaxByOrDefault(static e => e.Effect.DamagePerSecond.Amount.NumericValue)?.Effect;
        }

        protected override void StartScope(ref EffectChangeResult statusChange)
        {
            fireFlicker = new FireFlicker();
            Target.AddComponent(fireFlicker);
            statusChange = new ElementalStatus("fire".ToStatusIconSpriteId());
        }

        protected override void StartEffect(Effect effect, ref EffectChangeResult statusChange)
        {
        }


        protected override void ApplyEffectTick(Effect effect)
        {
            var damage = effect.DamagePerSecond * TickDuration;
            DamageExecutor.FromDamageSource(effect.DamageSource)
                .TryDoDamage(Target, damage.Typed(DamageType.Fire), Hit.FromSelf());
        }

        protected override void EndEffect()
        {
        }

        protected override void EndScope()
        {
            if (fireFlicker == null)
            {
                throw new InvalidOperationException("Cannot end effect that was not started.");
            }
            Target.RemoveComponent(fireFlicker);
            fireFlicker = null;
        }
    }
}
