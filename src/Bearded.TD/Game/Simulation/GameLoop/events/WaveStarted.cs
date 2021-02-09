using Bearded.TD.Game.Simulation.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameLoop
{
    readonly struct WaveStarted : IGlobalEvent
    {
        public Id<WaveScript> WaveId { get; }
        public string WaveName { get; }

        public WaveStarted(Id<WaveScript> waveId, string waveName)
        {
            WaveId = waveId;
            WaveName = waveName;
        }
    }
}
