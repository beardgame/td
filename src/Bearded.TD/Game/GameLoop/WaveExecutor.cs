using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.GameLoop;

sealed class WaveExecutor : IListener<WaveEnded>
{
    private readonly GameState game;
    private readonly IdManager idManager;
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;

    private readonly Dictionary<Wave, VoidEventHandler> activeWaves = new();

    private bool subscribed;

    public WaveExecutor(GameState game, IdManager idManager, ICommandDispatcher<GameInstance> commandDispatcher)
    {
        this.game = game;
        this.idManager = idManager;
        this.commandDispatcher = commandDispatcher;
    }

    public void ExecuteScript(WaveScript script, VoidEventHandler onComplete)
    {
        subscribeEventsIfNeeded();

        var spawnedObjectIds = idManager.GetBatch<GameObject>(script.EnemyCount);
        var wave = new Wave(idManager.GetNext<Wave>(), script, spawnedObjectIds, game.Time);
        activeWaves.Add(wave, onComplete);

        commandDispatcher.Dispatch(ExecuteWave.Command(game, wave));
    }

    private void subscribeEventsIfNeeded()
    {
        if (subscribed)
        {
            return;
        }

        game.Meta.Events.Subscribe(this);
        subscribed = true;
    }

    public void HandleEvent(WaveEnded @event)
    {
        if (!activeWaves.TryGetValue(@event.Wave, out var onComplete))
        {
            return;
        }

        onComplete();
        activeWaves.Remove(@event.Wave);
    }
}
