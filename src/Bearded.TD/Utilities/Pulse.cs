using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities
{
    sealed class Pulse : IPulse
    {
        private readonly ITimeSource timeSource;
        private readonly TimeSpan timeBetweenBeats;
        private Instant lastBeat;

        public event VoidEventHandler? Heartbeat;

        public Pulse(ITimeSource timeSource, TimeSpan timeBetweenBeats)
        {
            this.timeSource = timeSource;
            this.timeBetweenBeats = timeBetweenBeats;
        }

        public void Update()
        {
            if (timeSource.Time - lastBeat <= timeBetweenBeats)
            {
                return;
            }

            Heartbeat?.Invoke();
            lastBeat = timeSource.Time;
        }
    }

    interface IPulse
    {
        public event VoidEventHandler? Heartbeat;
    }
}
