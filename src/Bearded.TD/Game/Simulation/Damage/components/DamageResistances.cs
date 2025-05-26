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
        ImmutableDictionary<DamageType, Resistance>? Resistances { get; }
    }

    public DamageResistances(ImmutableDictionary<DamageType, Resistance> resistances)
        : this(new DamageResistancesParametersTemplate(resistances))
    {
    }

    protected override DamageShell AffectedShell => DamageShell.Health;

    public override void ModifyDamage(ref DamagePreview preview)
    {
        if (Parameters.Resistances?.TryGetValue(preview.DamageType, out var resistance) ?? false)
        {
            preview.Resist(resistance);
        }
    }
}
