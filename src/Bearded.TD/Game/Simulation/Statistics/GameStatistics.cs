using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics;

interface IGameStatistics
{
    void RegisterDamage(Id<GameObject> id, FinalDamageResult damageResult);
}

sealed class GameStatistics : IGameStatistics, IListener<WaveStarted>, IListener<WaveEnded>
{
    private readonly Dictionary<Id<GameObject>, TowerStatistics> statsByTower = new();

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
        var waveReport = WaveReport.Create(
            statsByTower
                .Select(kvp => (Id: kvp.Key, DamageByType: kvp.Value.ToTypedAccumulatedDamages()))
                .ToImmutableArray());
        events.Send(new WaveReportCreated(@event.WaveId, waveReport));

#if DEBUG
        // TODO: delete debug code
        foreach (var (id, stats) in statsByTower)
        {
            var list = stats.ToTypedAccumulatedDamages().Select(toString).ToList();

            Console.WriteLine($"{id}");
            foreach (var s in list)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine();
        }

        return;

        string toString(WaveReport.TypedAccumulatedDamage tad)
        {
            return $"{tad.Type} -> {tad.DamageDone} / {tad.AttemptedDamage} ({tad.Efficiency:P})";
        }
#endif
    }

    public void RegisterDamage(Id<GameObject> id, FinalDamageResult damageResult)
    {
        if (!statsByTower.TryGetValue(id, out var statistics))
        {
            statistics = new TowerStatistics();
            statsByTower.Add(id, statistics);
        }

        statistics.RegisterDamage(
            damageResult.TotalExactDamage.Untyped(),
            damageResult.AttemptedDamage.Untyped(),
            damageResult.TotalExactDamage.Type);
    }

    private sealed class TowerStatistics
    {
        private readonly Dictionary<DamageType, WaveReport.AccumulatedDamage> accumulatedDamageByType = new();

        public void RegisterDamage(UntypedDamage damageDone, UntypedDamage damageAttempted, DamageType damageType)
        {
            var existingDamage =
                accumulatedDamageByType.GetValueOrDefault(damageType, WaveReport.AccumulatedDamage.Zero);
            var addedDamage = new WaveReport.AccumulatedDamage(damageDone, damageAttempted);
            accumulatedDamageByType[damageType] = WaveReport.AccumulatedDamage.Combine(existingDamage, addedDamage);
        }

        public ImmutableArray<WaveReport.TypedAccumulatedDamage> ToTypedAccumulatedDamages() =>
            accumulatedDamageByType
                .Select(kvp => new WaveReport.TypedAccumulatedDamage(kvp.Key, kvp.Value))
                .ToImmutableArray();
    }
}
