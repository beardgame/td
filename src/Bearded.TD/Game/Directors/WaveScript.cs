using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Resources;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Directors
{
    sealed record WaveScript
    {
        public Id<WaveScript> Id { get; }
        public Faction TargetFaction { get; }
        public Instant SpawnStart { get; }
        public TimeSpan SpawnDuration { get; }
        public ResourceAmount ResourcesAwardedBySpawnPhase { get; }

        public Instant SpawnEnd => SpawnStart + SpawnDuration;

        public WaveScript(
            Id<WaveScript> id,
            Faction targetFaction,
            Instant spawnStart,
            TimeSpan spawnDuration,
            ResourceAmount resourcesAwardedBySpawnPhase)
        {
            Id = id;
            TargetFaction = targetFaction;
            SpawnStart = spawnStart;
            SpawnDuration = spawnDuration;
            ResourcesAwardedBySpawnPhase = resourcesAwardedBySpawnPhase;
        }
    }
}
