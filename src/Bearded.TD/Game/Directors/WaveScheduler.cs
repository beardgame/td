using Bearded.Utilities;

namespace Bearded.TD.Game.Directors
{
    sealed class WaveScheduler
    {
        public event VoidEventHandler? WaveEnded;

        public WaveScheduler()
        {
        }

        public void StartWave(WaveRequirements waveRequirements)
        {
        }

        private void startSpawning()
        {
        }

        public readonly struct WaveRequirements
        {
        }
    }
}
