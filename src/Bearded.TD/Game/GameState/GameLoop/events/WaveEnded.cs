using Bearded.TD.Game.GameState.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.GameState.GameLoop
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
