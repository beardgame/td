using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed record WaveScript(
    Id<WaveScript> Id,
    string DisplayName,
    Faction TargetFaction,
    Instant? SpawnStart,
    TimeSpan SpawnDuration,
    ImmutableArray<SpawnLocation> SpawnLocations,
    EnemySpawnScript EnemyScript,
    ImmutableArray<Id<GameObject>> SpawnedUnitIds);
