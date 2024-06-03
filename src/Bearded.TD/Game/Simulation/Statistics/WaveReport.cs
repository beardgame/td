using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics.Data;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class WaveReport
{
    private readonly ImmutableDictionary<GameObject, TowerStatistics> statsByTower;
    public ImmutableArray<TypedAccumulatedDamage> AccumulatedDamageByType { get; }

    public TowerStatistics StatsForTower(GameObject obj) => statsByTower[obj];
    public IEnumerable<TowerStatistics> AllTowers => statsByTower.Values;

    public static WaveReport Empty { get; } = new(
        ImmutableDictionary<GameObject, TowerStatistics>.Empty,
        ImmutableArray<TypedAccumulatedDamage>.Empty
    );

    public static WaveReport Create(IEnumerable<TowerStatistics> stats)
    {
        var towerData = stats as ICollection<TowerStatistics> ?? stats.ToList();
        var statsByTower = towerData.ToImmutableDictionary(data => data.GameObject);
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
        ImmutableDictionary<GameObject, TowerStatistics> statsByTower,
        ImmutableArray<TypedAccumulatedDamage> accumulatedDamageByType)
    {
        this.statsByTower = statsByTower;
        AccumulatedDamageByType = accumulatedDamageByType;
    }
}
