using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Statistics;

interface IGameStatistics
{
    void RegisterDamage(GameObject obj, FinalDamageResult damageResult);
}

sealed class GameStatistics : IGameStatistics, IListener<WaveStarted>, IListener<WaveEnded>
{
    private readonly Dictionary<GameObject, TowerStatistics> statsByTower = new();

    public static IGameStatistics CreateSubscribed(IDispatcher<GameInstance> dispatcher, GlobalGameEvents events)
    {
        var instance = new GameStatistics(dispatcher, events);
        events.Subscribe<WaveStarted>(instance);
        events.Subscribe<WaveEnded>(instance);
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
        var waveReport = WaveReport.Create(statsByTower.Select(kvp => kvp.Value.ToStats()));
        events.Send(new WaveReportCreated(@event.WaveId, waveReport));
    }

    public void RegisterDamage(GameObject obj, FinalDamageResult damageResult)
    {
        if (!statsByTower.TryGetValue(obj, out var statistics))
        {
            statistics = new TowerStatistics(obj);
            statsByTower.Add(obj, statistics);
        }

        statistics.RegisterDamage(
            damageResult.TotalExactDamage.Untyped(),
            damageResult.AttemptedDamage.Untyped(),
            damageResult.TotalExactDamage.Type);
    }

    private sealed class TowerStatistics(GameObject obj)
    {
        private readonly Dictionary<DamageType, AccumulatedDamage> accumulatedDamageByType = new();

        public void RegisterDamage(UntypedDamage damageDone, UntypedDamage damageAttempted, DamageType damageType)
        {
            var existingDamage =
                accumulatedDamageByType.GetValueOrDefault(damageType, AccumulatedDamage.Zero);
            var addedDamage = new AccumulatedDamage(damageDone, damageAttempted);
            accumulatedDamageByType[damageType] = AccumulatedDamage.Combine(existingDamage, addedDamage);
        }

        public Data.TowerStatistics ToStats() => new()
        {
            GameObject = obj,
            DamageByType = toTypedAccumulatedDamages()
        };

        private ImmutableArray<TypedAccumulatedDamage> toTypedAccumulatedDamages() =>
            accumulatedDamageByType
                .Select(kvp => new TypedAccumulatedDamage(kvp.Key, kvp.Value))
                .ToImmutableArray();
    }
}
