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
    Instant SpawnStart,
    TimeSpan SpawnDuration,
    ResourceAmount ResourcesAwarded,
    ImmutableArray<SpawnLocation> SpawnLocations,
    EnemySpawnScript EnemyScript,
    IGameObjectBlueprint UnitBlueprint,
    ImmutableArray<Id<GameObject>> SpawnedUnitIds)
{
    public Instant SpawnEnd => SpawnStart + SpawnDuration;

}
