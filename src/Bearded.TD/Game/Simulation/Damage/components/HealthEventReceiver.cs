using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class HealthEventReceiver : Component, IHealthEventReceiver
{
    public TypedDamage Damage(TypedDamage typedDamage, IDamageSource? source)
    {
        var previewDamage = new PreviewTakeDamage(typedDamage);
        Events.Preview(ref previewDamage);

        var modifiedDamage = typedDamage;
        if (previewDamage.Resistance is { } resistance)
        {
            modifiedDamage = resistance.ApplyToDamage(modifiedDamage);
        }
        if (previewDamage.DamageCap is { } damageCap && damageCap < modifiedDamage.Amount)
        {
            modifiedDamage = modifiedDamage.WithAdjustedAmount(damageCap);
        }

        if (modifiedDamage.Amount > HitPoints.Zero)
        {
            Events.Send(new TakeDamage(modifiedDamage, source));
        }

        return modifiedDamage;
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
