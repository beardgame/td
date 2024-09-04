using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.UpdateLoop;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class WaveDirector
{
    private readonly GameState game;
    private readonly DeletableObjectList<SingleWaveDirector> waves = new();

    public WaveDirector(GameState game)
    {
        this.game = game;
    }

    public void ExecuteWave(Wave wave)
    {
        var waveDirector = new SingleWaveDirector(game, wave);
        waves.Add(waveDirector);
        waveDirector.Start();
    }

    public void Update()
    {
        foreach (var wave in waves)
        {
            wave.Update();
        }
    }

    private sealed class SingleWaveDirector : IListener<EnemyKilled>, IListener<WaveTimerSkipRequested>, IDeletable
    {
        private enum Phase
        {
            NotStarted,
            Downtime,
            AwaitingSpawnStartRequirements,
            Spawning,
            FinishOff,
            Completed,
        }

        private readonly GameState game;
        private readonly Wave wave;
        private readonly Queue<EnemySpawn> spawnQueue = new();
        private readonly HashSet<GameObject> spawnedUnits = new();
        private readonly List<ISpawnStartRequirement> outstandingSpawnStartRequirements = new();

        private Phase phase;

        private Instant? actualSpawnStart;

        private bool canSummonNow => phase == Phase.Downtime && outstandingSpawnStartRequirements.Count == 0;

        public bool Deleted => phase == Phase.Completed;

        public SingleWaveDirector(GameState game, Wave wave)
        {
            this.game = game;
            this.wave = wave;
            actualSpawnStart = wave.Script.DowntimeDuration == null
                ? null
                : wave.DowntimeStart + wave.Script.DowntimeDuration;
        }

        public void Start()
        {
            fillSpawnQueue();
            game.Meta.Events.Subscribe<EnemyKilled>(this);
            game.Meta.Events.Subscribe<WaveTimerSkipRequested>(this);
            game.Meta.Events.Send(
                new WaveScheduled(
                    wave.Id,
                    wave.Script.DisplayName,
                    actualSpawnStart,
                    outstandingSpawnStartRequirements.Add,
                    () => canSummonNow));
            phase = Phase.Downtime;
            var spawnsByLocation = wave.Script.EnemyScript.SpawnEvents.ToLookup(e => e.SpawnLocation, e => e.EnemyForm);
            foreach (var location in wave.Script.SpawnLocations)
            {
                location.UpdateSpawnTile();
                location.AssignWave(wave.Id, spawnsByLocation[location]);
            }
        }

        private void fillSpawnQueue()
        {
            var idIndex = 0;
            foreach (var spawnEvent in wave.Script.EnemyScript.SpawnEvents)
            {
                spawnQueue.Enqueue(
                    new EnemySpawn(
                        wave.SpawnedObjectIds[idIndex++],
                        spawnEvent.EnemyForm,
                        spawnEvent.SpawnLocation,
                        spawnEvent.TimeOffset));
            }
        }

        public void Update()
        {
            outstandingSpawnStartRequirements.RemoveAll(r => r.Satisfied);

            switch (phase)
            {
                case Phase.Downtime:
                    if (actualSpawnStart is not null && game.Time >= actualSpawnStart)
                    {
                        tryStartSpawningPhase();
                    }
                    break;
                case Phase.AwaitingSpawnStartRequirements:
                    if (!game.GameTime.IsPaused)
                    {
                        startSpawningPhase();
                    }
                    break;
                case Phase.Spawning:
                    updateSpawnQueue();
                    if (spawnQueue.Count == 0)
                    {
                        phase = Phase.FinishOff;
                    }
                    break;
                case Phase.FinishOff:
                    if (spawnedUnits.Count == 0)
                    {
                        finishWave();
                    }
                    break;
                case Phase.Completed:
                case Phase.NotStarted:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void finishWave()
        {
            game.Meta.Events.Send(new WaveEnded(wave, wave.Script.TargetFaction));
            game.Meta.Events.Unsubscribe<EnemyKilled>(this);
            game.Meta.Events.Unsubscribe<WaveTimerSkipRequested>(this);
            phase = Phase.Completed;
        }

        private void tryStartSpawningPhase()
        {
            if (outstandingSpawnStartRequirements.Count == 0)
            {
                startSpawningPhase();
                return;
            }

            game.GameTime.PauseUntil(PauseCondition.UntilTrue(() => outstandingSpawnStartRequirements.Count == 0));
            game.Meta.Events.Send(new WaveDeferred());
            phase = Phase.AwaitingSpawnStartRequirements;
        }

        private void startSpawningPhase()
        {
            phase = Phase.Spawning;
            game.Meta.Events.Send(new WaveStarted(wave.Id, wave.Script.DisplayName));
            updateSpawnQueue();
        }

        private void updateSpawnQueue()
        {
            State.Satisfies(actualSpawnStart is not null);
            var timePassedSinceStart = game.Time - actualSpawnStart;
            while (spawnQueue.TryPeek(out var spawn) && timePassedSinceStart >= spawn.TimeSinceStart)
            {
                spawnQueue.Dequeue();
                var unit =
                    EnemyFactory.Create(spawn.UnitId, spawn.EnemyForm, spawn.SpawnLocation.SpawnTile);
                game.Add(unit);
                spawnedUnits.Add(unit);
                spawn.SpawnLocation.OnEnemySpawned(spawn.EnemyForm);
            }
        }

        public void HandleEvent(EnemyKilled @event)
        {
            spawnedUnits.Remove(@event.Unit);
        }

        public void HandleEvent(WaveTimerSkipRequested @event)
        {
            if (phase != Phase.Downtime)
                return;

            actualSpawnStart = game.Time;
        }
    }

    private sealed record EnemySpawn(
        Id<GameObject> UnitId,
        EnemyForm EnemyForm,
        SpawnLocation SpawnLocation,
        TimeSpan TimeSinceStart);
}
