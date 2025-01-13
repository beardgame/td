using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class GameProgression : IListener<WaveScheduled>, IListener<WaveStarted>
{
    public int CurrentChapter { get; private set; }
    public int ChapterCount { get; private set; }
    public int CurrentWave { get; private set; }
    public int WaveCount { get; private set; }
    public double WaveProgress { get; private set; }

    public GameProgression(GlobalGameEvents events)
    {
        events.Subscribe<WaveScheduled>(this);
        events.Subscribe<WaveStarted>(this);
    }

    public void SetGameDuration(int chapterCount, int waveCount)
    {
        ChapterCount = chapterCount;
        WaveCount = waveCount;
    }

    public void HandleEvent(WaveScheduled @event)
    {
        CurrentChapter = @event.Wave.Script.ChapterNumber;
        CurrentWave = @event.Wave.Script.WaveNumber;
        WaveProgress = 0;
    }

    public void HandleEvent(WaveStarted @event)
    {
        @event.Progress.AddProgressObserver(onWaveProgressed, onWaveFinished);
    }

    private void onWaveProgressed(double newProgress)
    {
        WaveProgress = newProgress;
    }

    private void onWaveFinished()
    {
        WaveProgress = 1;
    }
}
