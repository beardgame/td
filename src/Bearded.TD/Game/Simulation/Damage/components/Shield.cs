using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("shield")]
sealed class Shield(Shield.IParameters parameters)
    : DamageModifier<Shield.IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(15)]
        UntypedDamage Threshold { get; }

        [Modifiable(0.1)]
        double EffectivenessOverThreshold { get; }
    }

    protected override DamageShell AffectedShell => DamageShell.Shield;

    public override void ModifyDamage(ref DamagePreview preview)
    {
        preview.ReduceDamageOverThreshold(Parameters.Threshold, Parameters.EffectivenessOverThreshold);
    }
}
