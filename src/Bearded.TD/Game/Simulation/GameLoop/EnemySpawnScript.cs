using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed record EnemySpawnScript(ImmutableArray<EnemySpawnScript.EnemySpawnEvent> SpawnEvents)
{
    public readonly record struct EnemySpawnEvent(
        SpawnLocation SpawnLocation, TimeSpan TimeOffset, EnemyForm EnemyForm);
}

sealed record EnemySpawnTimes(ImmutableArray<EnemySpawnTimes.EnemySpawnTime> SpawnTimes)
{
    public readonly record struct EnemySpawnTime(TimeSpan TimeOffset, EnemyForm EnemyForm);
}
