using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Directors
{
    sealed class WaveDirector
    {
        public event VoidEventHandler? WaveEnded;

        public void StartWave(WaveRequirements waveRequirements)
        {

        }

        public void Update(TimeSpan elapsedTime)
        {

        }

        public readonly struct WaveRequirements
        {
        }
    }
}
