using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Statistics.Data;

sealed record AccumulatedDamage(UntypedDamage DamageDone, UntypedDamage DamagePotential)
{
    public double Efficiency => DamagePotential <= UntypedDamage.Zero ? 0 : DamageDone / DamagePotential;

    public static AccumulatedDamage Zero => new(UntypedDamage.Zero, UntypedDamage.Zero);

    public static AccumulatedDamage Combine(AccumulatedDamage left, AccumulatedDamage right) =>
        new(left.DamageDone + right.DamageDone, left.DamagePotential + right.DamagePotential);

    public static AccumulatedDamage Aggregate(IEnumerable<AccumulatedDamage> enumerable) =>
        enumerable.Aggregate(Zero, Combine);
}
