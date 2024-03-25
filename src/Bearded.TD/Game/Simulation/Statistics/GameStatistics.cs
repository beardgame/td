using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class GameStatistics : IListener<WaveStarted>, IListener<WaveEnded>
{
    private readonly Dictionary<Id<GameObject>, TowerStatistics> statsByTower = new();

    public static GameStatistics CreateSubscribed(GlobalGameEvents events)
    {
        var instance = new GameStatistics();
        events.Subscribe<WaveStarted>(instance);
        events.Subscribe<WaveEnded>(instance);
        return instance;
    }

    private GameStatistics() {}

    public void HandleEvent(WaveStarted @event)
    {
        statsByTower.Clear();
    }

    public void HandleEvent(WaveEnded @event)
    {
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
        public void RegisterDamage(UntypedDamage damageDone, UntypedDamage damageAttempted, DamageType damageType)
        {
        }
    }
}
