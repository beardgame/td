using Bearded.TD.Game.Factions;
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
        public double ResourcesAwardedBySpawnPhase { get; }

        public Instant SpawnEnd => SpawnStart + SpawnDuration;

        public WaveScript(
            Id<WaveScript> id,
            Faction targetFaction,
            Instant spawnStart,
            TimeSpan spawnDuration,
            double resourcesAwardedBySpawnPhase)
        {
            Id = id;
            TargetFaction = targetFaction;
            SpawnStart = spawnStart;
            SpawnDuration = spawnDuration;
            ResourcesAwardedBySpawnPhase = resourcesAwardedBySpawnPhase;
        }
    }
}
