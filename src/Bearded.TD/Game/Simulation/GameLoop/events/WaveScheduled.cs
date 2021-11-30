using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop
{
    delegate void SpawnStartRequirementConsumer(ISpawnStartRequirement requirement);

    readonly struct WaveScheduled : IGlobalEvent
    {
        public Id<WaveScript> WaveId { get; }
        public string WaveName { get; }
        public Instant SpawnStart { get; }
        public ResourceAmount ResourceAmount { get; }
        public SpawnStartRequirementConsumer SpawnStartRequirementConsumer { get; }

        public WaveScheduled(
            Id<WaveScript> waveId,
            string waveName,
            Instant spawnStart,
            ResourceAmount resourceAmount,
            SpawnStartRequirementConsumer spawnStartRequirementConsumer)
        {
            WaveId = waveId;
            WaveName = waveName;
            SpawnStart = spawnStart;
            ResourceAmount = resourceAmount;
            SpawnStartRequirementConsumer = spawnStartRequirementConsumer;
        }
    }
}
