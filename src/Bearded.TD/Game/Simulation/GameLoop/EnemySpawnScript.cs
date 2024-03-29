using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed record EnemySpawnScript(ImmutableArray<EnemySpawnScript.EnemySpawnEvent> SpawnEvents)
{
    private static readonly Utilities.Comparer<EnemySpawnEvent> eventComparer = Utilities.Comparer<EnemySpawnEvent>
        .Comparing(e => e.TimeOffset)
        .ThenComparing(e => e.SpawnLocation.Id.Value);

    public readonly record struct EnemySpawnEvent(
        SpawnLocation SpawnLocation, TimeSpan TimeOffset, EnemyForm EnemyForm);

    public static EnemySpawnScript Merge(IEnumerable<EnemySpawnScript> scripts)
    {
        var events = scripts
            .SelectMany(s => s.SpawnEvents)
            .OrderBy(e => e, eventComparer)
            .ToImmutableArray();
        return new EnemySpawnScript(events);
    }
}

sealed record EnemySpawnTimes(ImmutableArray<EnemySpawnTimes.EnemySpawnTime> SpawnTimes)
{
    public readonly record struct EnemySpawnTime(TimeSpan TimeOffset, EnemyForm EnemyForm);
}
