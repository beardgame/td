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
    private sealed class Scope : ElementalPhenomenonScopeBase<Effect>
    {
        private FireFlicker? fireFlicker;

        public Scope(GameObject target) : base(target) { }

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

        protected override void ApplyEffectTick(GameObject target, Effect effect)
        {
            var damage = effect.DamagePerSecond * TickDuration;
            DamageExecutor.FromDamageSource(effect.DamageSource)
                .TryDoDamage(target, damage.Typed(DamageType.Fire), Hit.FromSelf());
        }

        protected override void StartEffect(GameObject target)
        {
            fireFlicker = new FireFlicker();
            target.AddComponent(fireFlicker);
        }

        protected override void EndEffect(GameObject target)
        {
            if (fireFlicker == null)
            {
                throw new InvalidOperationException("Cannot end effect that was not started.");
            }
            target.RemoveComponent(fireFlicker);
            fireFlicker = null;
        }

        protected override ElementalStatus MakeStatus(Blueprints blueprints)
        {
            return new ElementalStatus("fire".ToStatusIconSpriteId());
        }
    }
}
