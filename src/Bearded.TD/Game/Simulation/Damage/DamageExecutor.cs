using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DamageExecutor<T> : Component<T>, IDamageExecutor
    {
        protected override void Initialize() {}

        public DamageResult Damage(DamageInfo damageInfo)
        {
            var takeDamage = new TakeDamage(damageInfo);
            Events.Preview(ref takeDamage);
            var result = new DamageResult(takeDamage.DamageTaken);
            Events.Send(new TookDamage(damageInfo.Source, result));
            return result;
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }

    interface IDamageExecutor
    {
        DamageResult Damage(DamageInfo damageInfo);
    }
}
