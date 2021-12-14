using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Units;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed record WaveScript(
    Id<WaveScript> Id,
    string DisplayName,
    Faction TargetFaction,
    Instant SpawnStart,
    TimeSpan SpawnDuration,
    ResourceAmount ResourcesAwardedBySpawnPhase,
    ImmutableArray<SpawnLocation> SpawnLocations,
    int UnitsPerSpawnLocation,
    IUnitBlueprint UnitBlueprint,
    ImmutableArray<Id<EnemyUnit>> SpawnedUnitIds)
{
    public Instant SpawnEnd => SpawnStart + SpawnDuration;

}