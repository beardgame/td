using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("armor")]
sealed class Armor : HitPointsPool<Armor.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        HitPoints MaxHitPoints { get; }

        [Modifiable(15)]
        HitPoints BlockedDamageAmount { get; }

        [Modifiable(0.1)]
        double BlockedDamageEffectiveness { get; }
    }

    protected override HitPoints TargetMaxHitPoints => Parameters.MaxHitPoints;
    public override DamageShell Shell => DamageShell.Armor;

    public Armor(IParameters parameters) : base(parameters, parameters.MaxHitPoints) { }

    protected override void OnAdded() { }

    protected override TypedDamage ModifyDamage(TypedDamage damage)
    {
        var blockedAmount = SpaceTime1MathF.Min(damage.Amount, Parameters.BlockedDamageAmount);
        var passedThroughAmount = damage.Amount - blockedAmount;

        return damage.WithAdjustedAmount(blockedAmount * (float) Parameters.BlockedDamageEffectiveness + passedThroughAmount);
    }
}
