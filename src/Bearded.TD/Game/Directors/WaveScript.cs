using Bearded.TD.Game.Factions;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Directors
{
    sealed record WaveScript
    {
        // TODO: figure out why the compiler gets so upset when making this an Id<WaveScript>
        public Id<WaveScript> Id { get; }
        public Faction TargetFaction { get; }
        public Instant SpawnStart { get; }
        public Instant SpawnEnd { get; }
        public double ResourcesAwardedOverTime { get; }

        public TimeSpan SpawnDuration => SpawnEnd - SpawnStart;

        public WaveScript(
            Id<WaveScript> id,
            Faction targetFaction,
            Instant spawnStart,
            Instant spawnEnd,
            double resourcesAwardedOverTime)
        {
            Id = id;
            TargetFaction = targetFaction;
            SpawnStart = spawnStart;
            SpawnEnd = spawnEnd;
            ResourcesAwardedOverTime = resourcesAwardedOverTime;
        }
    }
}
