using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameLoop;

readonly struct WaveStarted : IGlobalEvent
{
    public Id<Wave> WaveId { get; }
    public string WaveName { get; }

    public WaveStarted(Id<Wave> waveId, string waveName)
    {
        WaveId = waveId;
        WaveName = waveName;
    }
}
