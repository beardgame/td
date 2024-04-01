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
    private readonly ImmutableArray<TypedAccumulatedDamage> accumulatedDamageByType;

    public Tower StatsForTower(Id<GameObject> id) => statsByTower.GetValueOrDefault(id, Tower.NoDamage);

    public static WaveReport Empty = new(
        ImmutableDictionary<Id<GameObject>, Tower>.Empty,
        ImmutableArray<TypedAccumulatedDamage>.Empty
    );

    public static WaveReport Create(
        ImmutableArray<(Id<GameObject> Id, ImmutableArray<TypedAccumulatedDamage> DamageByType)> stats)
    {
        var statsByTower = stats.ToImmutableDictionary(tuple => tuple.Id, tuple => new Tower(tuple.DamageByType));
        var damageByType = stats
            .SelectMany(tuple => tuple.DamageByType)
            .GroupBy(
                d => d.Type,
                (type, damages) => new TypedAccumulatedDamage(
                    type,
                    AccumulatedDamage.Aggregate(damages.Select(t => t.AccumulatedDamage))))
            .ToImmutableArray();
        return new WaveReport(statsByTower, damageByType);
    }

    private WaveReport(
        ImmutableDictionary<Id<GameObject>, Tower> statsByTower,
        ImmutableArray<TypedAccumulatedDamage> accumulatedDamageByType)
    {
        this.statsByTower = statsByTower;
        this.accumulatedDamageByType = accumulatedDamageByType;
    }

    public sealed record Tower(ImmutableArray<TypedAccumulatedDamage> DamageByType)
    {
        // TODO: consider precalculating these
        public UntypedDamage TotalDamageDone =>
            AccumulatedDamage.Aggregate(DamageByType.Select(d => d.AccumulatedDamage)).DamageDone;
        public double TotalEfficiency =>
            AccumulatedDamage.Aggregate(DamageByType.Select(d => d.AccumulatedDamage)).Efficiency;

        public static Tower NoDamage => new(ImmutableArray<TypedAccumulatedDamage>.Empty);
    }

    public sealed record TypedAccumulatedDamage(DamageType Type, AccumulatedDamage AccumulatedDamage)
    {
        public UntypedDamage DamageDone => AccumulatedDamage.DamageDone;
        public UntypedDamage AttemptedDamage => AccumulatedDamage.AttemptedDamage;
        public float Efficiency => AccumulatedDamage.Efficiency;
    }

    public sealed record AccumulatedDamage(UntypedDamage DamageDone, UntypedDamage AttemptedDamage)
    {
        public float Efficiency => AttemptedDamage <= UntypedDamage.Zero ? 1 : DamageDone / AttemptedDamage;

        public static AccumulatedDamage Zero => new(UntypedDamage.Zero, UntypedDamage.Zero);

        public static AccumulatedDamage Combine(AccumulatedDamage left, AccumulatedDamage right) =>
            new(left.DamageDone + right.DamageDone, left.AttemptedDamage + right.AttemptedDamage);

        public static AccumulatedDamage Aggregate(IEnumerable<AccumulatedDamage> enumerable) =>
            enumerable.Aggregate(Zero, Combine);
    }
}
