using Bearded.Utilities;

namespace Bearded.TD.Game.GameLoop;

sealed class ChapterDirector
{
    private readonly WaveScheduler waveScheduler;

    public ChapterDirector(WaveScheduler waveScheduler)
    {
        this.waveScheduler = waveScheduler;
    }

    public void ExecuteScript(ChapterScript script, VoidEventHandler onComplete)
    {
        var chapter = new SingleChapterDirector(waveScheduler, script);
        chapter.Start();
        chapter.Completed += onComplete;
    }

    private sealed class SingleChapterDirector
    {
        private readonly WaveScheduler waveScheduler;
        private readonly ChapterScript script;

        private int wavesSpawned;

        public event VoidEventHandler? Completed;

        public SingleChapterDirector(WaveScheduler waveScheduler, ChapterScript script)
        {
            this.waveScheduler = waveScheduler;
            this.script = script;
        }

        public void Start()
        {
            requestWave();
        }

        private void onWaveEnded()
        {
            if (wavesSpawned < script.WaveCount)
            {
                requestWave();
            }
            else
            {
                finish();
            }
        }

        private void requestWave()
        {
            var description = script.Waves[wavesSpawned];
            var waveNumber = ++wavesSpawned;
            var requirements = toRequirements(waveNumber, description);
            waveScheduler.StartWave(requirements, onWaveEnded);
        }

        private WaveRequirements toRequirements(int waveNumber, WaveDescription waveDescription)
        {
            return new WaveRequirements(
                script.ChapterNumber,
                waveNumber,
                new WaveEnemyComposition(waveDescription.TotalThreat, script.Elements),
                waveDescription.DownTimeDuration);
        }

        private void finish()
        {
            Completed?.Invoke();
        }
    }
}
