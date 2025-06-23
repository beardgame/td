using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.TechEffects;
using static Bearded.TD.Game.Simulation.Modules.ReflectRemainingDamage;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("reflectDamage")]
sealed class ReflectRemainingDamage(IParameters parameters) : DamageModifier<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        DamageShell Shell { get; }
        DamageType DamageType { get; }
        UntypedDamage MinimumDamageToReflect { get; }

        [Modifiable(1)]
        double Factor { get; }
    }

    protected override DamageShell AffectedShell => Parameters.Shell;

    public override void ModifyDamage(ref DamagePreview preview)
    {
        if (preview.DamageType == Parameters.DamageType &&
            preview.DamageAmount >= Parameters.MinimumDamageToReflect)
        {
            preview.ReflectDamage(Parameters.Factor);
        }
    }
}
