using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage
{
    interface IDamageSource
    {
        void AttributeDamage(IDamageTarget target, DamageResult result);
        void AttributeKill(IDamageTarget target);
    }

    sealed class DamageSource<T> : Component<T>, IDamageSource
        where T : IIdable<T>
    {
        public Id<T> Id => Owner.Id;

        public void AttributeDamage(IDamageTarget target, DamageResult damageResult)
        {
            Events.Send(new CausedDamage(target, damageResult));
        }

        public void AttributeKill(IDamageTarget target)
        {
            Events.Send(new CausedKill(target));
        }

        protected override void OnAdded() { }
        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(CoreDrawers drawers) { }
    }
}
