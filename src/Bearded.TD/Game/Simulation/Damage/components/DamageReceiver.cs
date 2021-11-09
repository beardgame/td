using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DamageReceiver<T> : Component<T>, IDamageReceiver
    {
        protected override void OnAdded() {}

        public DamageResult Damage(DamageInfo damageInfo)
        {
            var previewDamage = new PreviewTakeDamage(damageInfo);
            Events.Preview(ref previewDamage);

            var modifiedDamageInfo = damageInfo;
            if (previewDamage.DamageCap is { } damageCap && damageCap < modifiedDamageInfo.Amount)
            {
                modifiedDamageInfo = damageInfo.WithAdjustedAmount(damageCap);
            }
            var result = new DamageResult(modifiedDamageInfo);

            if (modifiedDamageInfo.Amount > HitPoints.Zero)
            {
                Events.Send(new TakeDamage(result));
                damageInfo.Source?.AttributeDamage(result);
            }
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
