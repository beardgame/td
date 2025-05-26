using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using static Bearded.TD.Game.Simulation.Modules.PierceDamageThroughShell;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("pierceDamageThroughShell")]
sealed class PierceDamageThroughShell(IParameters parameters) : DamageModifier<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        DamageShell Shell { get; }
        DamageType DamageType { get; }
        double Factor { get; }
    }

    protected override DamageShell AffectedShell => Parameters.Shell;

    public override void ModifyDamage(ref DamagePreview preview)
    {
        if (preview.DamageType == Parameters.DamageType)
            preview.PierceDamageToNextShell(Parameters.Factor);
    }
}
