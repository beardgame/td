using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class HealthEventReceiver : Component, IHealthEventReceiver
{
    public void Damage(DamageInfo damageInfo, IDamageSource? source)
    {
        var previewDamage = new PreviewTakeDamage(damageInfo);
        Events.Preview(ref previewDamage);

        var modifiedDamageInfo = damageInfo;
        if (previewDamage.DamageCap is { } damageCap && damageCap < modifiedDamageInfo.Amount)
        {
            modifiedDamageInfo = damageInfo.WithAdjustedAmount(damageCap);
        }

        if (modifiedDamageInfo.Amount > HitPoints.Zero)
        {
            var result = new DamageResult(modifiedDamageInfo);
            Events.Send(new TakeDamage(result, source));
        }
    }

    public void Heal(HealInfo healInfo)
    {
        var previewHeal = new PreviewHealDamage(healInfo);
        Events.Preview(ref previewHeal);

        var modifiedHealInfo = healInfo;
        if (previewHeal.HealCap is { } healCap && healCap < modifiedHealInfo.Amount)
        {
            modifiedHealInfo = healInfo.WithAdjustedAmount(healCap);
        }

        if (modifiedHealInfo.Amount > HitPoints.Zero)
        {
            var result = new HealResult(modifiedHealInfo);
            Events.Send(new HealDamage(result));
        }
    }

    protected override void OnAdded() {}

    public override void Update(TimeSpan elapsedTime) {}
}

interface IHealthEventReceiver
{
    void Damage(DamageInfo damageInfo, IDamageSource? source);
    void Heal(HealInfo healInfo);
}
