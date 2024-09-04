using Bearded.Utilities;

namespace Bearded.TD.Game.GameLoop;

sealed class ChapterExecutor
{
    private readonly SpawnLocationActivator spawnLocationActivator;
    private readonly WaveGenerator waveGenerator;
    private readonly WaveExecutor waveExecutor;

    public ChapterExecutor(
        SpawnLocationActivator spawnLocationActivator, WaveGenerator waveGenerator, WaveExecutor waveExecutor)
    {
        this.spawnLocationActivator = spawnLocationActivator;
        this.waveGenerator = waveGenerator;
        this.waveExecutor = waveExecutor;
    }

    public void ExecuteScript(ChapterScript script, VoidEventHandler onComplete)
    {
        var chapter = new SingleChapterExecutor(spawnLocationActivator, waveGenerator, waveExecutor, script);
        chapter.Start();
        chapter.Completed += onComplete;
    }

    private sealed class SingleChapterExecutor
    {
        private readonly SpawnLocationActivator spawnLocationActivator;
        private readonly WaveGenerator waveGenerator;
        private readonly WaveExecutor waveExecutor;
        private readonly ChapterScript script;

        private int wavesSpawned;

        public event VoidEventHandler? Completed;

        public SingleChapterExecutor(
            SpawnLocationActivator spawnLocationActivator,
            WaveGenerator waveGenerator,
            WaveExecutor waveExecutor,
            ChapterScript script)
        {
            this.spawnLocationActivator = spawnLocationActivator;
            this.waveGenerator = waveGenerator;
            this.waveExecutor = waveExecutor;
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
            spawnLocationActivator.WakeSpawnLocation();
            var availableSpawnLocations = spawnLocationActivator.GatherAvailableSpawnLocations();
            var waveScript = waveGenerator.GenerateWave(requirements, availableSpawnLocations);
            waveExecutor.ExecuteScript(waveScript, onWaveEnded);
        }

        private WaveRequirements toRequirements(int waveNumber, WaveDescription waveDescription)
        {
            var structure = WaveStructure.FromTemplate(
                WaveTemplates.Chapter[(waveNumber - 1) % WaveTemplates.Chapter.Length],
                new WaveEnemyComposition(waveDescription.TotalThreat, script.Elements));
            return new WaveRequirements(
                script.ChapterNumber,
                waveNumber,
                waveNumber == script.WaveCount,
                structure,
                waveDescription.DownTimeDuration);
        }

        private void finish()
        {
            Completed?.Invoke();
        }
    }
}
