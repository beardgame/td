using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class Overdrive<T> : Component<T>
        where T : IComponentOwner<T>, IGameObject
    {
        private static readonly TimeSpan damageInterval = 0.5.S();
        private static float damagePercentile = 0.025f;

        private IUpgradeEffect fireRate = null!;
        private IUpgradeEffect damageOverTime = null!;

        private Instant? nextDamageTime;

        protected override void OnAdded()
        {
            var ids = Owner.Game.GamePlayIds;

            fireRate = new ParameterModifiableWithId(AttributeType.FireRate, damageModification(ids));
            damageOverTime = new ParameterModifiableWithId(AttributeType.DamageOverTime, damageModification(ids));

            fireRate.ApplyTo(Owner);
            damageOverTime.ApplyTo(Owner);
        }

        private static ModificationWithId damageModification(IdManager ids)
            => new(ids.GetNext<Modification>(), Modification.MultiplyWith(1.5));

        public override void OnRemoved()
        {
            fireRate.RemoveFrom(Owner);
            damageOverTime.RemoveFrom(Owner);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (isAnyTriggerPulled(Owner))
            {
                if (nextDamageTime is not { } damageTime)
                {
                    damageOwnerAtTime(Owner.Game.Time);
                }
                else if (damageTime <= Owner.Game.Time)
                {
                    damageOwnerAtTime(damageTime);
                }
            }
            else
            {
                nextDamageTime = null;
            }
        }

        private bool isAnyTriggerPulled(IComponentOwner owner)
        {
            return owner.GetComponents<IComponent>().Any(
                c =>
                    c is IWeaponTrigger { TriggerPulled: true } ||
                    c is INestedComponentOwner nested && isAnyTriggerPulled(nested.NestedComponentOwner)
            );
        }

        private void damageOwnerAtTime(Instant time)
        {
            var damage = new HitPoints(
                Owner.TryGetSingleComponent<IHealth>(out var health)
                    ? MoreMath.CeilToInt(health.MaxHealth.NumericValue * damagePercentile)
                    : 10);

            var overdriveDamage = new DamageInfo(damage, DamageType.DivineIntervention);

            var success = DamageExecutor.FromDamageSource(null).TryDoDamage(Owner, overdriveDamage);

            nextDamageTime = time + damageInterval;
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}
