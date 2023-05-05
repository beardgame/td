using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveScheduler
{
    public sealed record WaveRequirements(
        int ChapterNumber,
        int WaveNumber,
        WaveEnemyComposition EnemyComposition,
        TimeSpan? DowntimeDuration);

    public sealed record WaveEnemyComposition(
        double TotalThreat,
        ElementalTheme Elements);

    private record struct WaveParameters(
        EnemyForm EnemyForm,
        int EnemiesPerSpawn,
        ImmutableArray<SpawnLocation> SpawnLocations,
        TimeSpan SpawnDuration);
}
