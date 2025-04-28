using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("armor")]
sealed class Armor(Armor.IParameters parameters) : HitPointsPool<Armor.IParameters>(parameters, parameters.MaxHitPoints)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        HitPoints MaxHitPoints { get; }

        [Modifiable(15)]
        HitPoints BlockedDamageAmount { get; }

        [Modifiable(0.1)]
        double BlockedDamageEffectiveness { get; }

        [Modifiable(0.5)]
        double LightningPiercingFactor { get; }
    }

    protected override HitPoints TargetMaxHitPoints => Parameters.MaxHitPoints;
    public override DamageShell Shell => DamageShell.Armor;
    protected override Color Color => Constants.Game.GameUI.ArmorColor;

    protected override void OnAdded() { }

    protected override TypedDamage ModifyDamage(
        TypedDamage damage, out IReadOnlyList<AdditionalHitEffect> additionalEffects)
    {
        var blockedAmount = SpaceTime1MathF.Min(damage.Amount, Parameters.BlockedDamageAmount);

        var preview = new ArmorDamagePreview(
            damage,
            blockedAmount,
            (float) Parameters.BlockedDamageEffectiveness,
            // Is it nice that this is hardcoded? Maybe not, but it's also inherent armour behaviour ¯\_(ツ)_/¯
            damage.Type == DamageType.Lightning ? (float) Parameters.LightningPiercingFactor : 0);
        additionalEffects = preview.AdditionalEffects;

        var damageAfterArmor = DamageCalculations.FlatArmourBonus(damage.Untyped(), Parameters.BlockedDamageAmount, Parameters.BlockedDamageEffectiveness, preview.PiercingFactor);
        var resistance = preview.DamageResistance ?? Resistance.Zero;
        var damageAfterResistance = resistance.ApplyToDamage(damageAfterArmor);

        return damageAfterResistance.Typed(damage.Type);
    }
}
