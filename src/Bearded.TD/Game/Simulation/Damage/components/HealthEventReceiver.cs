using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class HealthEventReceiver : Component, IHealthEventReceiver
{
    public TypedDamage Damage(TypedDamage typedDamage, IDamageSource? source)
    {
        var previewDamage = new PreviewTakeDamage(typedDamage);
        Events.Preview(ref previewDamage);

        var modifiedDamageInfo = typedDamage;
        if (previewDamage.DamageCap is { } damageCap && damageCap < modifiedDamageInfo.Amount)
        {
            modifiedDamageInfo = typedDamage.WithAdjustedAmount(damageCap);
        }

        if (modifiedDamageInfo.Amount > HitPoints.Zero)
        {
            var result = new DamageResult(modifiedDamageInfo);
            Events.Send(new TakeDamage(result, source));
        }

        return modifiedDamageInfo;
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
    TypedDamage Damage(TypedDamage typedDamage, IDamageSource? source);
    void Heal(HealInfo healInfo);
}
