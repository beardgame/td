using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed record WaveScript(
    string DisplayName,
    Faction TargetFaction,
    TimeSpan? DowntimeDuration,
    TimeSpan SpawnDuration,
    ImmutableArray<SpawnLocation> SpawnLocations,
    EnemySpawnScript EnemyScript);
