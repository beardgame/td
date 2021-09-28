using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DamageReceiver<T> : Component<T>, IDamageReceiver
    {
        protected override void OnAdded() {}

        public DamageResult Damage(DamageInfo damageInfo)
        {
            var takeDamage = new TakeDamage(damageInfo);
            Events.Preview(ref takeDamage);
            var result = new DamageResult(takeDamage.Damage.WithAdjustedAmount(takeDamage.DamageTaken));
            Events.Send(new TookDamage(damageInfo.Source, result));
            damageInfo.Source?.AttributeDamage(result);
            return result;
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }

    interface IDamageReceiver
    {
        DamageResult Damage(DamageInfo damageInfo);
    }
}
