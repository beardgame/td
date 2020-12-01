using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Directors.WaveDirector;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Directors
{
    sealed class ChapterDirector
    {
        private readonly WaveDirector waveDirector;

        private bool chapterStarted;
        private int wavesLeftInChapter;

        public event VoidEventHandler? ChapterEnded;

        public ChapterDirector(WaveDirector waveDirector)
        {
            this.waveDirector = waveDirector;
            waveDirector.WaveEnded += onWaveEnded;
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
            waveDirector.StartWave(new WaveRequirements());
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (!chapterStarted) return;

            waveDirector.Update(elapsedTime);
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
