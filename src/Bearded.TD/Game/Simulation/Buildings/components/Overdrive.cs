using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class Overdrive<T> : Component<T>
        where T : IComponentOwner<T>, IGameObject
    {
        private IUpgradeEffect fireRate = null!;
        private IUpgradeEffect damageOverTime = null!;

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
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}
