using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("armor")]
sealed class Armor(Armor.IParameters parameters) : DamageModifier<Armor.IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(15)]
        HitPoints BlockedDamageAmount { get; }

        [Modifiable(0.1)]
        double BlockedDamageEffectiveness { get; }

        [Modifiable(0.1)]
        double LightningPiercingFactor { get; }
    }

    protected override DamageShell AffectedShell => DamageShell.Armor;

    public override void ModifyDamage(ref DamagePreview preview)
    {
        // Is it nice that this is hardcoded? Maybe not, but it's also inherent armour behaviour ¯\_(ツ)_/¯
        if (preview.DamageType == DamageType.Lightning)
        {
            preview.PierceDamageToNextShell(Parameters.LightningPiercingFactor);
        }

        preview.ReduceDamageUnderThreshold(Parameters.BlockedDamageAmount, Parameters.BlockedDamageEffectiveness);
    }
}
