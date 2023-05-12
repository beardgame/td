using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameLoop;

sealed record WaveDescription(
    double TotalThreat,
    TimeSpan? DownTimeDuration);
