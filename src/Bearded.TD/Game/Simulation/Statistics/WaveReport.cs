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
    public ImmutableArray<TypedAccumulatedDamage> AccumulatedDamageByType { get; }

    public Tower StatsForTower(Id<GameObject> id) => statsByTower[id];
    public IEnumerable<Tower> AllTowers => statsByTower.Values;

    public static WaveReport Empty { get; } = new(
        ImmutableDictionary<Id<GameObject>, Tower>.Empty,
        ImmutableArray<TypedAccumulatedDamage>.Empty
    );

    public record struct TowerData(
        Id<GameObject> Id,
        GameObject GameObject,
        ImmutableArray<TypedAccumulatedDamage> DamageByType
    );

    public static WaveReport Create(IEnumerable<TowerData> stats)
    {
        var towerData = stats as ICollection<TowerData> ?? stats.ToList();
        var statsByTower = towerData.ToImmutableDictionary(data => data.Id, Tower.From);
        var damageByType = towerData
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
        AccumulatedDamageByType = accumulatedDamageByType;
    }

    public sealed record Tower(GameObject GameObject, ImmutableArray<TypedAccumulatedDamage> DamageByType)
    {
        public static Tower From(TowerData data) => new(data.GameObject, data.DamageByType);

        // TODO: consider precalculating these
        public UntypedDamage TotalDamageDone =>
            AccumulatedDamage.Aggregate(DamageByType.Select(d => d.AccumulatedDamage)).DamageDone;

        public double TotalEfficiency =>
            AccumulatedDamage.Aggregate(DamageByType.Select(d => d.AccumulatedDamage)).Efficiency;
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
