using Bearded.Utilities;
using static Bearded.TD.Game.Directors.WaveScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Directors
{
    sealed class ChapterScheduler
    {
        private readonly WaveScheduler waveScheduler;

        private bool chapterStarted;
        private int wavesLeftInChapter;

        public event VoidEventHandler? ChapterEnded;

        public ChapterScheduler(WaveScheduler waveScheduler)
        {
            this.waveScheduler = waveScheduler;
            waveScheduler.WaveEnded += onWaveEnded;
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
            wavesLeftInChapter = chapterRequirements.WaveCount;
        }

        private void endChapter()
        {
            State.Satisfies(chapterStarted);
            chapterStarted = false;
            ChapterEnded?.Invoke();
        }

        private void requestWave()
        {
            State.Satisfies(wavesLeftInChapter > 0);
            wavesLeftInChapter--;
            waveScheduler.StartWave(new WaveRequirements());
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
