using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Statistics.Data;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class WaveReport
{
    // TODO: what do we need this for; what do we key this by; ids are unhappy because of the fake data in report screen
    private readonly ImmutableDictionary<TowerMetadata, TowerStatistics> statsByTower;
    public ImmutableArray<TypedAccumulatedDamage> AccumulatedDamageByType { get; }

    public IEnumerable<TowerStatistics> AllTowers => statsByTower.Values;

    public static WaveReport Empty { get; } = new(
        ImmutableDictionary<TowerMetadata, TowerStatistics>.Empty,
        ImmutableArray<TypedAccumulatedDamage>.Empty
    );

    public static WaveReport Create(IEnumerable<TowerStatistics> stats)
    {
        var towerData = stats as ICollection<TowerStatistics> ?? stats.ToList();
        var statsByTower = towerData.ToImmutableDictionary(data => data.Metadata);
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
        ImmutableDictionary<TowerMetadata, TowerStatistics> statsByTower,
        ImmutableArray<TypedAccumulatedDamage> accumulatedDamageByType)
    {
        this.statsByTower = statsByTower;
        AccumulatedDamageByType = accumulatedDamageByType;
    }
}
