using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics;

interface IGameStatistics
{
    TowerMetadata FindTowerMetadata(Id<GameObject> id);
    void RegisterDamage(GameObject obj, FinalDamageResult damageResult);
}

sealed class GameStatistics
    : IGameStatistics, IListener<WaveStarted>, IListener<WaveEnded>, IListener<BuildingConstructionFinished>
{
    private readonly TowerArchive towerArchive = new();
    private readonly Dictionary<GameObject, TowerStatistics> statsByTower = new();

    public static IGameStatistics CreateSubscribed(IDispatcher<GameInstance> dispatcher, GlobalGameEvents events)
    {
        var instance = new GameStatistics(dispatcher, events);
        events.Subscribe<WaveStarted>(instance);
        events.Subscribe<WaveEnded>(instance);
        events.Subscribe<BuildingConstructionFinished>(instance);
        return instance;
    }

    private readonly IDispatcher<GameInstance> dispatcher;
    private readonly GlobalGameEvents events;

    private GameStatistics(IDispatcher<GameInstance> dispatcher, GlobalGameEvents events)
    {
        this.dispatcher = dispatcher;
        this.events = events;
    }

    public void HandleEvent(WaveStarted @event)
    {
        statsByTower.Clear();
    }

    public void HandleEvent(WaveEnded @event)
    {
        // TODO: synchronize using dispatcher
        var waveReport = WaveReport.Create(statsByTower.Select(kvp => kvp.Value.ToStats(towerArchive.Find(kvp.Key))));
        events.Send(new WaveReportCreated(@event.WaveId, waveReport));
    }

    public void HandleEvent(BuildingConstructionFinished @event)
    {
        towerArchive.EnsureTowerExists(@event.GameObject);
    }

    public TowerMetadata FindTowerMetadata(Id<GameObject> id) => towerArchive.Find(id);

    public void RegisterDamage(GameObject obj, FinalDamageResult damageResult)
    {
        if (!statsByTower.TryGetValue(obj, out var statistics))
        {
            if (towerArchive.EnsureTowerExists(obj))
            {
                DebugAssert.State.IsInvalid("Tower should have been archived when it was finished building.");
            }
            statistics = new TowerStatistics(obj.FindId());
            statsByTower.Add(obj, statistics);
        }

        statistics.RegisterDamage(
            damageResult.TotalExactDamage.Untyped(),
            damageResult.AttemptedDamage.Untyped(),
            damageResult.TotalExactDamage.Type);
    }

    private sealed class TowerStatistics(Id<GameObject> id)
    {
        private readonly Dictionary<DamageType, AccumulatedDamage> accumulatedDamageByType = new();

        public void RegisterDamage(UntypedDamage damageDone, UntypedDamage damageAttempted, DamageType damageType)
        {
            var existingDamage =
                accumulatedDamageByType.GetValueOrDefault(damageType, AccumulatedDamage.Zero);
            var addedDamage = new AccumulatedDamage(damageDone, damageAttempted);
            accumulatedDamageByType[damageType] = AccumulatedDamage.Combine(existingDamage, addedDamage);
        }

        public Data.TowerStatistics ToStats(TowerMetadata metadata) => new()
        {
            Metadata = metadata,
            DamageByType = toTypedAccumulatedDamages()
        };

        private ImmutableArray<TypedAccumulatedDamage> toTypedAccumulatedDamages() =>
            accumulatedDamageByType
                .Select(kvp => new TypedAccumulatedDamage(kvp.Key, kvp.Value))
                .ToImmutableArray();
    }
}
