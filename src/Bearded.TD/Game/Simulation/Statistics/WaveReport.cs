using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class WaveReport
{
    private readonly ImmutableDictionary<Id<GameObject>, Tower> statsByTower;
    private readonly ImmutableArray<TypedDamageDone> damageByType;

    public Tower StatsForTower(Id<GameObject> id) => statsByTower.GetValueOrDefault(id, Tower.NoDamage);

    public static WaveReport Create(
        ImmutableArray<(Id<GameObject> Id, ImmutableArray<TypedDamageDone> DamageByType)> stats)
    {
        var statsByTower = stats.ToImmutableDictionary(tuple => tuple.Id, tuple => new Tower(tuple.DamageByType));
        var damageByType = stats
            .SelectMany(tuple => tuple.DamageByType)
            .GroupBy(
                d => d.Type,
                (type, damages) => new TypedDamageDone(type, DamageDone.Aggregate(damages.Select(t => t.Damage))))
            .ToImmutableArray();
        return new WaveReport(statsByTower, damageByType);
    }

    private WaveReport(
        ImmutableDictionary<Id<GameObject>, Tower> statsByTower, ImmutableArray<TypedDamageDone> damageByType)
    {
        this.statsByTower = statsByTower;
        this.damageByType = damageByType;
    }

    public sealed record Tower(ImmutableArray<TypedDamageDone> DamageByType)
    {
        // TODO: consider precalculating these
        public UntypedDamage TotalDamageDone => DamageDone.Aggregate(DamageByType.Select(d => d.Damage)).TotalDamage;
        public double TotalEfficiency => DamageDone.Aggregate(DamageByType.Select(d => d.Damage)).Efficiency;

        public static Tower NoDamage => new(ImmutableArray<TypedDamageDone>.Empty);
    }

    public sealed record TypedDamageDone(DamageType Type, DamageDone Damage)
    {
        public UntypedDamage TotalDamage => Damage.TotalDamage;
        public float Efficiency => Damage.Efficiency;
    }

    public sealed record DamageDone(UntypedDamage TotalDamage, float Efficiency)
    {
        public static DamageDone Zero => new(UntypedDamage.Zero, 1f);

        public static DamageDone Combine(DamageDone left, DamageDone right)
        {
            var totalDamage = left.TotalDamage + right.TotalDamage;
            var weightedLeftEfficiency = left.TotalDamage * left.Efficiency;
            var weightedRightEfficiency = right.TotalDamage * right.Efficiency;
            var weightedEfficiency = (weightedLeftEfficiency + weightedRightEfficiency) / totalDamage;
            return new DamageDone(totalDamage, weightedEfficiency);
        }

        public static DamageDone Aggregate(IEnumerable<DamageDone> enumerable) => enumerable.Aggregate(Zero, Combine);
    }
}
