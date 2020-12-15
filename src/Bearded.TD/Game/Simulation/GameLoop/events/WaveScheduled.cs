using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop
{
    sealed record WaveScheduled : IGlobalEvent
    {
        public Id<WaveScript> WaveId { get; }
        public int WaveNumber { get; }
        public Instant SpawnStart { get; }
        public ResourceAmount ResourceAmount { get; }

        public WaveScheduled(Id<WaveScript> waveId, int waveNumber, Instant spawnStart, ResourceAmount resourceAmount)
        {
            WaveId = waveId;
            WaveNumber = waveNumber;
            SpawnStart = spawnStart;
            ResourceAmount = resourceAmount;
        }
    }
}
