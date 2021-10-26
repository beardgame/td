using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage
{
    interface IDamageSource
    {
        void AttributeDamage(DamageResult result);
        void AttributeKill(IDamageTarget target);
    }

    sealed class DamageSource<T> : Component<T>, IDamageSource
        where T : IComponentOwner
    {
        private IIdProvider<T>? idProvider;
        public Id<T> Id => idProvider?.Id ?? Id<T>.Invalid;

        protected override void OnAdded()
        {
            ComponentDependencies.Depend<IIdProvider<T>>(Owner, Events, provider => idProvider = provider);
        }

        public void AttributeDamage(DamageResult damageResult)
        {
            Events.Send(new AttributedDamage(damageResult));
        }

        public void AttributeKill(IDamageTarget target)
        {
            Events.Send(new AttributedKill(target));
        }

        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(CoreDrawers drawers) { }
    }
}
