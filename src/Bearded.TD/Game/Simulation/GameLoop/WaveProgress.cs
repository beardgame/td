using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Disposables;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.UI;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class WaveProgress
{
    public delegate void OnWaveProgressed(double progress);
    public delegate void OnWaveFinished();
    private readonly record struct ProgressObserver(OnWaveProgressed OnWaveProgressed, OnWaveFinished OnWaveFinished);

    private readonly int enemiesToKillCount;
    private readonly HashSet<Id<GameObject>> enemiesLeftToKill;
    private bool scriptedEventQueueDirty;
    private readonly List<ScriptedEvent> scriptedEventQueue = [];
    private readonly List<ProgressObserver> progressObservers = [];

    private int enemiesKilledCount => enemiesToKillCount - enemiesLeftToKill.Count;

    public static WaveProgress FromWave(GameState game, Wave wave, out IDisposable disposable)
    {
        var progress = new WaveProgress(wave.SpawnedObjectIds);

        disposable = new CompositeDisposable(
            game.Meta.Events.Observe<EnemyKilled>().Subscribe(progress.onEnemyKilled),
            game.Meta.Events.Observe<WaveEnded>().Subscribe(progress.onWaveEnded)
        );

        return progress;
    }

    private WaveProgress(ImmutableArray<Id<GameObject>> enemiesToKill)
    {
        enemiesToKillCount = enemiesToKill.Length;
        enemiesLeftToKill = [..enemiesToKill];
    }

    public void AddScriptedEvent(double progress, Action execute)
    {
        scriptedEventQueue.Add(new ScriptedEvent(progress, execute));
        scriptedEventQueueDirty = true;
    }

    public void AddProgressObserver(OnWaveProgressed onProgressed, OnWaveFinished onFinished)
    {
        progressObservers.Add(new ProgressObserver(onProgressed, onFinished));
    }

    private void onEnemyKilled(EnemyKilled @event)
    {
        enemiesLeftToKill.Remove(@event.Unit.FindId());
        processScriptedEventQueue();
        notifyObserversOfProgress();
    }

    private void onWaveEnded(WaveEnded @event)
    {
        DebugAssert.State.Satisfies(enemiesLeftToKill.Count == 0);
        flushScriptedEventQueue();
        notifyObserversOfCompletion();
    }

    private void processScriptedEventQueue()
    {
        if (scriptedEventQueueDirty)
        {
            scriptedEventQueue.Sort(Comparer.Comparing<ScriptedEvent, double>(e => e.Progress));
            scriptedEventQueueDirty = false;
        }

        var currentProgress = (double) enemiesKilledCount / enemiesToKillCount;
        var eventsProcessed = 0;
        for (var i = 0; i < scriptedEventQueue.Count; i++)
        {
            if (currentProgress < scriptedEventQueue[i].Progress)
            {
                break;
            }

            scriptedEventQueue[i].Execute();
            eventsProcessed++;
        }

        scriptedEventQueue.RemoveRange(0, eventsProcessed);
    }

    private void notifyObserversOfProgress()
    {
        var progress = (double) enemiesKilledCount / enemiesToKillCount;
        foreach (var observer in progressObservers)
        {
            observer.OnWaveProgressed(progress);
        }
    }

    private void flushScriptedEventQueue()
    {
        foreach (var e in scriptedEventQueue)
        {
            e.Execute();
        }
        scriptedEventQueue.Clear();
    }

    private void notifyObserversOfCompletion()
    {
        foreach (var observer in progressObservers)
        {
            observer.OnWaveFinished();
        }
    }

    private readonly record struct ScriptedEvent(double Progress, Action Execute);
}
