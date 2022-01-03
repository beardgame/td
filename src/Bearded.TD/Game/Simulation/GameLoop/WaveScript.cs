using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
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
    ResourceAmount ResourcesAwardedBySpawnPhase,
    ImmutableArray<SpawnLocation> SpawnLocations,
    int UnitsPerSpawnLocation,
    IComponentOwnerBlueprint UnitBlueprint,
    ImmutableArray<Id<ComponentGameObject>> SpawnedUnitIds)
{
    public Instant SpawnEnd => SpawnStart + SpawnDuration;

}
