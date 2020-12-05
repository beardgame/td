using Bearded.TD.Game.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Directors
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
