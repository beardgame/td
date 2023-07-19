using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed record WaveScript(
    string DisplayName,
    Faction TargetFaction,
    TimeSpan? DowntimeDuration,
    ImmutableArray<SpawnLocation> SpawnLocations,
    EnemySpawnScript EnemyScript)
{
    public int EnemyCount => EnemyScript.SpawnEvents.Length;
}
