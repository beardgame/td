using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Units;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop
{
    sealed record WaveScript
    {
        public Id<WaveScript> Id { get; }
        public Faction TargetFaction { get; }
        public Instant SpawnStart { get; }
        public TimeSpan SpawnDuration { get; }
        public ResourceAmount ResourcesAwardedBySpawnPhase { get; }
        public ImmutableArray<SpawnLocation> SpawnLocations { get; }
        public int UnitsPerSpawnLocation { get; }
        public IUnitBlueprint UnitBlueprint { get; }
        public ImmutableArray<Id<EnemyUnit>> SpawnedUnitIds { get; }

        public Instant SpawnEnd => SpawnStart + SpawnDuration;

        public WaveScript(
            Id<WaveScript> id,
            Faction targetFaction,
            Instant spawnStart,
            TimeSpan spawnDuration,
            ResourceAmount resourcesAwardedBySpawnPhase,
            ImmutableArray<SpawnLocation> spawnLocations,
            int unitsPerSpawnLocation,
            IUnitBlueprint unitBlueprint,
            ImmutableArray<Id<EnemyUnit>> spawnedUnitIds)
        {
            Id = id;
            TargetFaction = targetFaction;
            SpawnStart = spawnStart;
            SpawnDuration = spawnDuration;
            ResourcesAwardedBySpawnPhase = resourcesAwardedBySpawnPhase;
            SpawnLocations = spawnLocations;
            UnitsPerSpawnLocation = unitsPerSpawnLocation;
            UnitBlueprint = unitBlueprint;
            SpawnedUnitIds = spawnedUnitIds;
        }
    }
}
