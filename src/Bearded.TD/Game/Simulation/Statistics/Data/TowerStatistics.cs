using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Statistics.Data;

readonly record struct TowerStatistics(GameObject GameObject, ImmutableArray<TypedAccumulatedDamage> DamageByType)
{
    // TODO: consider precalculating these (or maybe have a wrapper type that has all these precalculated)
    public UntypedDamage TotalDamageDone =>
        AccumulatedDamage.Aggregate(DamageByType.Select(d => d.AccumulatedDamage)).DamageDone;

    public double TotalEfficiency =>
        AccumulatedDamage.Aggregate(DamageByType.Select(d => d.AccumulatedDamage)).Efficiency;
}
