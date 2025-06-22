using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics.commands;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics;

interface IGameStatistics
{
    TowerMetadata FindTowerMetadata(Id<GameObject> id);
    void RegisterDamage(GameObject obj, FinalDamageResult damageResult);
    ITowerStatisticObserver ObserveTower(GameObject obj);
}

sealed class GameStatistics
    : IGameStatistics, IListener<WaveStarted>, IListener<WaveEnded>, IListener<BuildingConstructionFinished>
{
    private readonly TowerArchive towerArchive = new();
    private readonly Dictionary<GameObject, TowerStatistics> statsByTower = new();
    private readonly List<TowerStatisticObserver> observers = [];
    private readonly MultiDictionary<GameObject, TowerStatisticObserver> observersByTower = new();

    public static IGameStatistics CreateSubscribed(GameState gameState)
    {
        var instance = new GameStatistics(gameState);

        var events = gameState.Meta.Events;
        events.Subscribe<WaveStarted>(instance);
        events.Subscribe<WaveEnded>(instance);
        events.Subscribe<BuildingConstructionFinished>(instance);

        return instance;
    }

    private readonly GameState gameState;
    private IDispatcher<GameInstance> dispatcher => gameState.Meta.Dispatcher;

    private GameStatistics(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void HandleEvent(WaveStarted @event)
    {
        // Hold on to the list of observers to dispose.
        var observersToDispose = new List<TowerStatisticObserver>(observers);

        // Clear all the actual data structures. That way, observers can immediately start observing again and already
        // get a reference to the new tower statistics.
        statsByTower.Clear();
        observers.Clear();
        observersByTower.Clear();

        observersToDispose.ForEach(o => o.Dispose());
    }

    public void HandleEvent(WaveEnded @event)
    {
        var waveReport = WaveReport.Create(
            statsByTower
                .Where(kvp => kvp.Value.TotalDamage != null)
                .Select(kvp => kvp.Value.ToStats(towerArchive.Find(kvp.Key)))
        );
        dispatcher.RunOnlyOnServer(OpenWaveReport.Command, gameState, @event.Wave.Id, waveReport);
    }

    public void HandleEvent(BuildingConstructionFinished @event)
    {
        towerArchive.EnsureTowerExists(@event.GameObject);
    }

    public TowerMetadata FindTowerMetadata(Id<GameObject> id) => towerArchive.Find(id);

    public void RegisterDamage(GameObject obj, FinalDamageResult damageResult)
    {
        var statistics = findStatistics(obj);

        statistics.RegisterDamage(
            damageResult.TotalExactDamage.Untyped(),
            damageResult.ConsumedDamagePotential,
            damageResult.TotalExactDamage.Type);
        observersByTower[obj].ForEach(o => o.Notify());
    }

    public ITowerStatisticObserver ObserveTower(GameObject obj)
    {
        var statistics = findStatistics(obj);
        var observer = new TowerStatisticObserver(this, obj, statistics);
        observers.Add(observer);
        observersByTower.Add(obj, observer);
        return observer;
    }

    private TowerStatistics findStatistics(GameObject obj)
    {
        if (statsByTower.TryGetValue(obj, out var statistics)) return statistics;
        if (towerArchive.EnsureTowerExists(obj))
        {
            DebugAssert.State.IsInvalid("Tower should have been archived when it was finished building.");
        }
        statistics = new TowerStatistics();
        statsByTower.Add(obj, statistics);

        return statistics;
    }

    private void removeObserver(TowerStatisticObserver observer)
    {
        observers.Remove(observer);
        observersByTower.Remove(observer.Object, observer);
    }

    private sealed class TowerStatistics
    {
        private readonly Dictionary<DamageType, AccumulatedDamage> accumulatedDamageByType = new();

        public AccumulatedDamage? TotalDamage => accumulatedDamageByType.Count == 0 ? null : AccumulatedDamage.Aggregate(accumulatedDamageByType.Values);

        public void RegisterDamage(UntypedDamage damageDone, UntypedDamage damagePotential, DamageType damageType)
        {
            var existingDamage =
                accumulatedDamageByType.GetValueOrDefault(damageType, AccumulatedDamage.Zero);
            var addedDamage = new AccumulatedDamage(damageDone, damagePotential);
            accumulatedDamageByType[damageType] = AccumulatedDamage.Combine(existingDamage, addedDamage);
        }

        public Data.TowerStatistics ToStats(TowerMetadata metadata)
        {
            DebugAssert.State.Satisfies(accumulatedDamageByType.Count > 0,
                "Should not access statistics for a tower with no damage.");

            return new Data.TowerStatistics
            {
                Metadata = metadata,
                DamageByType = toTypedAccumulatedDamages(),
            };
        }

        private ImmutableArray<TypedAccumulatedDamage> toTypedAccumulatedDamages() =>
        [
            ..accumulatedDamageByType
                .Select(kvp => new TypedAccumulatedDamage(kvp.Key, kvp.Value)),
        ];
    }

    private sealed class TowerStatisticObserver(
        GameStatistics gameStatistics,
        GameObject obj,
        TowerStatistics towerStatistics) : ITowerStatisticObserver
    {
        public GameObject Object => obj;
        public AccumulatedDamage? TotalDamage => towerStatistics.TotalDamage;

        public event VoidEventHandler? StatisticsUpdated;
        public event VoidEventHandler? Disposed;

        public void StopObserving()
        {
            gameStatistics.removeObserver(this);
        }

        public void Notify()
        {
            StatisticsUpdated?.Invoke();
        }

        public void Dispose()
        {
            Disposed?.Invoke();
        }
    }
}
