using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop;

[Obsolete]
sealed class WaveScheduler : IListener<WaveEnded>
{
    private readonly GameState game;
    private readonly IdManager ids;
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;
    private readonly SpawnLocationActivator spawnLocationActivator;
    private readonly WaveGenerator waveGenerator;
    private readonly Faction targetFaction;
    private readonly Random random;

    private Wave? activeWave;

    public event VoidEventHandler? WaveEnded;

    public WaveScheduler(
        GameState game,
        IdManager ids,
        ICommandDispatcher<GameInstance> commandDispatcher,
        WaveGenerator waveGenerator,
        Faction targetFaction,
        int seed,
        SpawnLocationActivator spawnLocationActivator)
    {
        this.game = game;
        this.ids = ids;
        this.commandDispatcher = commandDispatcher;
        this.waveGenerator = waveGenerator;
        this.targetFaction = targetFaction;
        this.spawnLocationActivator = spawnLocationActivator;
        random = new Random(seed);
    }

    public void OnGameStart()
    {
        game.Meta.Events.Subscribe(this);
    }

    public void StartWave(WaveRequirements requirements, Action onComplete)
    {
        State.Satisfies(activeWave == null, "We only support one simultaneous wave right now");

        spawnLocationActivator.WakeSpawnLocation();
        var availableSpawnLocations = spawnLocationActivator.GatherAvailableSpawnLocations();

        var script = waveGenerator.GenerateWave(requirements, availableSpawnLocations, targetFaction);
        var spawnedObjectIds = ids.GetBatch<GameObject>(script.EnemyScript.SpawnEvents.Length);
        var wave = new Wave(ids.GetNext<Wave>(), script, spawnedObjectIds, game.Time);
        activeWave = wave;
        commandDispatcher.Dispatch(ExecuteWave.Command(game, wave));

        WaveEnded += onWaveEnd;

        void onWaveEnd()
        {
            onComplete();
            WaveEnded -= onWaveEnd;
        }
    }

    public void HandleEvent(WaveEnded @event)
    {
        if (@event.WaveId != activeWave?.Id)
        {
            return;
        }

        activeWave = null;
        WaveEnded?.Invoke();
    }
}
