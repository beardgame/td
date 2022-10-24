using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
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

    public void ExecuteScript(WaveScript script)
    {
        var wave = new SingleWaveDirector(game, script);
        waves.Add(wave);
        wave.Start();
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
        private readonly WaveScript script;
        private readonly Queue<EnemySpawn> spawnQueue = new();
        private readonly HashSet<GameObject> spawnedUnits = new();
        private readonly List<ISpawnStartRequirement> outstandingSpawnStartRequirements = new();

        private Phase phase;
        private ResourceAmount resourcesGiven;

        private TimeSpan skippedTime = TimeSpan.Zero;

        private Instant actualSpawnStart => script.SpawnStart - skippedTime;
        private Instant actualSpawnEnd => script.SpawnEnd - skippedTime;

        public bool Deleted => phase == Phase.Completed;

        public SingleWaveDirector(GameState game, WaveScript script)
        {
            this.game = game;
            this.script = script;
        }

        public void Start()
        {
            fillSpawnQueue();
            game.Meta.Events.Subscribe<EnemyKilled>(this);
            game.Meta.Events.Subscribe<WaveTimerSkipRequested>(this);
            game.Meta.Events.Send(
                new WaveScheduled(
                    script.Id,
                    script.DisplayName,
                    script.SpawnStart,
                    script.ResourcesAwardedBySpawnPhase,
                    outstandingSpawnStartRequirements.Add));
            phase = Phase.Downtime;
            foreach (var location in script.SpawnLocations)
            {
                location.UpdateSpawnTile();
                location.AssignWave(script.Id);
            }
        }

        private void fillSpawnQueue()
        {
            var idIndex = 0;
            foreach (var spawnEvent in script.EnemyScript.SpawnEvents)
            {
                foreach (var loc in script.SpawnLocations)
                {
                    spawnQueue.Enqueue(
                        new EnemySpawn(
                            script.SpawnedUnitIds[idIndex++],
                            script.UnitBlueprint,
                            loc,
                            script.SpawnStart + spawnEvent.TimeOffset));
                }
            }
        }

        public void Update()
        {
            outstandingSpawnStartRequirements.RemoveAll(r => r.Satisfied);

            switch (phase)
            {
                case Phase.Downtime:
                    if (game.Time >= actualSpawnStart)
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
                    updateResources();
                    updateSpawnQueue();
                    if (spawnQueue.Count == 0)
                    {
                        State.Satisfies(game.Time >= actualSpawnEnd);
                        phase = Phase.FinishOff;
                    }
                    break;
                case Phase.FinishOff:
                    if (spawnedUnits.Count == 0)
                    {
                        game.Meta.Events.Send(new WaveEnded(script.Id, script.TargetFaction));
                        game.Meta.Events.Unsubscribe<EnemyKilled>(this);
                        game.Meta.Events.Unsubscribe<WaveTimerSkipRequested>(this);
                        phase = Phase.Completed;
                    }
                    break;
                case Phase.Completed:
                case Phase.NotStarted:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            game.Meta.Events.Send(new WaveStarted(script.Id, script.DisplayName));
            updateResources();
            updateSpawnQueue();
        }

        private void updateResources()
        {
            var spawnTimeElapsed = game.Time - actualSpawnStart;
            var percentageTimeElapsed = Math.Clamp(spawnTimeElapsed / script.SpawnDuration, 0, 1);
            var expectedResourcesGiven = script.ResourcesAwardedBySpawnPhase.Percentage(percentageTimeElapsed);
            State.Satisfies(expectedResourcesGiven >= resourcesGiven);
            if (script.TargetFaction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                resources.ProvideResources(expectedResourcesGiven - resourcesGiven);
                resourcesGiven = expectedResourcesGiven;
            }
        }

        private void updateSpawnQueue()
        {
            while (spawnQueue.TryPeek(out var spawn) && game.Time >= spawn.Time)
            {
                spawnQueue.Dequeue();
                var unit =
                    EnemyUnitFactory.Create(spawn.UnitId, spawn.UnitBlueprint, spawn.SpawnLocation.SpawnTile);
                game.Add(unit);
                spawnedUnits.Add(unit);
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

            skippedTime = script.SpawnStart - game.Time;

            adjustSpawnQueueToStartNow();
        }

        private void adjustSpawnQueueToStartNow()
        {
            var queueLength = spawnQueue.Count;

            if (queueLength == 0)
                return;

            var timeToSkip = spawnQueue.Peek().Time - game.Time;

            for (var i = 0; i < queueLength; i++)
            {
                var oldSpawn = spawnQueue.Dequeue();
                var newSpawn = oldSpawn with { Time = oldSpawn.Time - timeToSkip };
                spawnQueue.Enqueue(newSpawn);
            }
        }
    }

    private sealed record EnemySpawn(
        Id<GameObject> UnitId,
        IGameObjectBlueprint UnitBlueprint,
        SpawnLocation SpawnLocation,
        Instant Time);
}
