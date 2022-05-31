using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class OnFire
{
    private sealed class Scope : ElementalPhenomenonScopeBase<Effect>
    {
        private FireFlicker? fireFlicker;

        protected override bool TryChooseEffect(out Effect effect)
        {
            var effects = ActiveEffects.ToImmutableArray();
            if (effects.IsEmpty)
            {
                effect = new Effect();
                return false;
            }

            effect = ActiveEffects.MaxBy(e => e.DamagePerSecond);
            return true;
        }

        protected override void ApplyEffectTick(IComponentOwner target, Effect effect)
        {
            DamageExecutor.FromDamageSource(effect.DamageSource)
                .TryDoDamage(target, new DamageInfo(effect.DamagePerSecond, DamageType.Fire));
        }

        protected override void StartEffect(IComponentOwner target)
        {
            fireFlicker = new FireFlicker();
            target.AddComponent(fireFlicker);
        }

        protected override void EndEffect(IComponentOwner target)
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
