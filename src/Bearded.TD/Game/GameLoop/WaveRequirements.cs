using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameLoop;

sealed record WaveRequirements(
    int ChapterNumber,
    int WaveNumber,
    WaveEnemyComposition EnemyComposition,
    TimeSpan? DowntimeDuration);
