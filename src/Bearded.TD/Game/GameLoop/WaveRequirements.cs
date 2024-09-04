using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.GameLoop.WaveStructure;

namespace Bearded.TD.Game.GameLoop;

sealed record WaveRequirements(
    int ChapterNumber,
    int WaveNumber,
    bool IsFinalWave,
    ScriptStructure Structure,
    TimeSpan? DowntimeDuration);
