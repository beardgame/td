using Bearded.TD.Game.Simulation.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameLoop
{
    readonly struct WaveEnded : IGlobalEvent
    {
        public Id<WaveScript> WaveId { get; }

        public WaveEnded(Id<WaveScript> waveId)
        {
            WaveId = waveId;
        }
    }
}
