using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("shield")]
sealed class Shield(Shield.IParameters parameters)
    : HitPointsPool<Shield.IParameters>(parameters, parameters.MaxHitPoints)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        HitPoints MaxHitPoints { get; }

        [Modifiable(15)]
        HitPoints DamageThreshold { get; }

        [Modifiable(0.1)]
        double BlockedDamageEffectiveness { get; }
    }

    protected override HitPoints TargetMaxHitPoints => Parameters.MaxHitPoints;
    public override DamageShell Shell => DamageShell.Shield;
    protected override Color Color => Constants.Game.GameUI.ShieldColor;

    protected override void OnAdded() { }

    protected override TypedDamage ModifyDamage(
        TypedDamage damage, out IReadOnlyList<AdditionalHitEffect> additionalEffects)
    {
        var fullDamageAmount = SpaceTime1MathF.Min(damage.Amount, Parameters.DamageThreshold);
        var blockedAmount = damage.Amount - fullDamageAmount;
        additionalEffects = [];

        return damage.WithAdjustedAmount(
            blockedAmount * (float) Parameters.BlockedDamageEffectiveness + fullDamageAmount);
    }
}
