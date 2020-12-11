using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop
{
    sealed class ChapterScheduler
    {
        private readonly WaveScheduler waveScheduler;

        private bool chapterStarted;
        private int wavesLeftInChapter;

        private bool firstWaveOfChapter;
        private double nextWaveValue = FirstWaveValue;
        private ResourceAmount nextWaveResources = FirstWaveResources;

        public event VoidEventHandler? ChapterEnded;

        public ChapterScheduler(WaveScheduler waveScheduler)
        {
            this.waveScheduler = waveScheduler;
            waveScheduler.WaveEnded += onWaveEnded;
        }

        public void OnGameStart()
        {
            waveScheduler.OnGameStart();
        }

        private void onWaveEnded()
        {
            if (wavesLeftInChapter > 0)
            {
                requestWave();
            }
            else
            {
                endChapter();
            }
        }

        public void StartChapter(ChapterRequirements chapterRequirements)
        {
            State.Satisfies(!chapterStarted);
            chapterStarted = true;

            wavesLeftInChapter = chapterRequirements.WaveCount;
            firstWaveOfChapter = true;
            requestWave();
        }

        private void requestWave()
        {
            State.Satisfies(wavesLeftInChapter > 0);
            wavesLeftInChapter--;
            waveScheduler.StartWave(new WaveRequirements(
                nextWaveValue,
                nextWaveResources,
                firstWaveOfChapter ? FirstDownTimeDuration : DownTimeDuration));

            firstWaveOfChapter = false;
            nextWaveValue *= WaveValueMultiplier;
            nextWaveResources *= WaveResourcesMultiplier;
        }

        private void endChapter()
        {
            State.Satisfies(chapterStarted);
            chapterStarted = false;
            ChapterEnded?.Invoke();
        }

        public readonly struct ChapterRequirements
        {
            public int WaveCount { get; }

            public ChapterRequirements(int waveCount)
            {
                WaveCount = waveCount;
            }
        }
    }
}
