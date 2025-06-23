using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using static Bearded.TD.Game.Simulation.Damage.DamageResistances;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("damageResistances")]
sealed class DamageResistances(IParameters parameters) : DamageModifier<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        DamageShell? Shell { get; }
        ImmutableDictionary<DamageType, Resistance>? Resistances { get; }
    }

    public static DamageResistances From(
        ImmutableDictionary<DamageType, Resistance> resistances, DamageShell shell = DamageShell.Health)
    {
        return new DamageResistances(new DamageResistancesParametersTemplate(shell, resistances));
    }

    protected override DamageShell AffectedShell => Parameters.Shell ?? DamageShell.Health;

    public override void ModifyDamage(ref DamagePreview preview)
    {
        if (Parameters.Resistances?.TryGetValue(preview.DamageType, out var resistance) ?? false)
        {
            preview.Resist(resistance);
        }
    }
}
