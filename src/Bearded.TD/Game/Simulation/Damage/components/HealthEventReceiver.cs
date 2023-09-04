using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class HealthEventReceiver : Component, IHealthEventReceiver
{
    public FinalDamageResult Damage(TypedDamage typedDamage, IDamageSource? source)
    {
        var damageReceivers =
            Owner.GetComponents<IDamageReceiver>().OrderByDescending(r => (int) r.Shell).ToImmutableArray();

        var remainingDamage = typedDamage;
        var totalExactDamage = TypedDamage.Zero(typedDamage.Type);
        var totalDiscreteDamage = HitPoints.Zero;

        foreach (var receiver in damageReceivers)
        {
            if (remainingDamage.Amount <= HitPoints.Zero)
            {
                break;
            }
            var intermediateResult = receiver.ApplyDamage(remainingDamage, source);
            remainingDamage = intermediateResult.DamageOverflow;
            totalExactDamage = totalExactDamage
                .WithAdjustedAmount(totalExactDamage.Amount + intermediateResult.ExactDamageDone.Amount);
            totalDiscreteDamage += intermediateResult.DiscreteDamageDone;
        }

        return new FinalDamageResult(totalExactDamage, totalDiscreteDamage);
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
    FinalDamageResult Damage(TypedDamage typedDamage, IDamageSource? source);
    void Heal(HealInfo healInfo);
}
