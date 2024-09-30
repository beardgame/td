using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class WaveProgress : IListener<EnemyKilled>, IListener<WaveEnded>
{
    private readonly GameState game;
    private readonly int enemiesToKillCount;
    private readonly HashSet<Id<GameObject>> enemiesLeftToKill;
    private bool scriptedEventQueueDirty;
    private readonly List<ScriptedEvent> scriptedEventQueue = [];

    private int enemiesKilledCount => enemiesToKillCount - enemiesLeftToKill.Count;

    public static WaveProgress FromWave(GameState game, Wave wave)
    {
        var progress = new WaveProgress(game, wave.SpawnedObjectIds);
        game.Meta.Events.Subscribe<EnemyKilled>(progress);
        game.Meta.Events.Subscribe<WaveEnded>(progress);
        return progress;
    }

    private WaveProgress(GameState game, ImmutableArray<Id<GameObject>> enemiesToKill)
    {
        this.game = game;
        enemiesToKillCount = enemiesToKill.Length;
        enemiesLeftToKill = [..enemiesToKill];
    }

    public void AddScriptedEvent(double progress, Action execute)
    {
        scriptedEventQueue.Add(new ScriptedEvent(progress, execute));
        scriptedEventQueueDirty = true;
    }

    public void HandleEvent(EnemyKilled @event)
    {
        enemiesLeftToKill.Remove(@event.Unit.FindId());
        processScriptedEventQueue();
    }

    public void HandleEvent(WaveEnded @event)
    {
        DebugAssert.State.Satisfies(enemiesLeftToKill.Count == 0);
        flushScriptedEventQueue();

        game.Meta.Events.Unsubscribe<EnemyKilled>(this);
        game.Meta.Events.Unsubscribe<WaveEnded>(this);
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
            if (currentProgress >= scriptedEventQueue[i].Progress)
            {
                eventsProcessed = i;
                break;
            }

            scriptedEventQueue[i].Execute();
        }

        scriptedEventQueue.RemoveRange(0, eventsProcessed);
    }

    private void flushScriptedEventQueue()
    {
        foreach (var e in scriptedEventQueue)
        {
            e.Execute();
        }
        scriptedEventQueue.Clear();
    }

    private readonly record struct ScriptedEvent(double Progress, Action Execute);
}
