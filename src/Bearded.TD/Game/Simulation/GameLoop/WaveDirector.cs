using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Units;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.GameLoop
{
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

        private sealed class SingleWaveDirector : IListener<EnemyKilled>, IDeletable
        {
            private enum Phase
            {
                NotStarted,
                Downtime,
                Spawning,
                FinishOff,
                Completed,
            }

            private readonly GameState game;
            private readonly WaveScript script;
            private readonly Queue<EnemySpawn> spawnQueue = new();
            private readonly HashSet<EnemyUnit> spawnedUnits = new();

            private Phase phase;
            private ResourceAmount resourcesGiven;

            public bool Deleted => phase == Phase.Completed;

            public SingleWaveDirector(GameState game, WaveScript script)
            {
                this.game = game;
                this.script = script;
            }

            public void Start()
            {
                fillSpawnQueue();
                game.Meta.Events.Subscribe(this);
                game.Meta.Events.Send(
                    new WaveScheduled(
                        script.Id, script.DisplayName, script.SpawnStart, script.ResourcesAwardedBySpawnPhase));
                phase = Phase.Downtime;
                foreach (var location in script.SpawnLocations)
                {
                    location.AssignWave(script.Id);
                }
            }

            private void fillSpawnQueue()
            {
                List<Instant> spawnTimes;
                if (script.UnitsPerSpawnLocation == 1)
                {
                    spawnTimes = new List<Instant>(script.SpawnStart.Yield());
                }
                else
                {
                    var timeBetweenSpawns = script.SpawnDuration / (script.UnitsPerSpawnLocation - 1);
                    spawnTimes = Enumerable.Range(0, script.UnitsPerSpawnLocation)
                        .Select(i => script.SpawnStart + i * timeBetweenSpawns)
                        .ToList();
                }

                var idIndex = 0;
                foreach (var time in spawnTimes)
                {
                    foreach (var loc in script.SpawnLocations)
                    {
                        spawnQueue.Enqueue(new EnemySpawn(script.SpawnedUnitIds[idIndex++], script.UnitBlueprint, loc, time));
                    }
                }
            }

            public void Update()
            {
                switch (phase)
                {
                    case Phase.Downtime:
                        if (game.Time >= script.SpawnStart)
                        {
                            phase = Phase.Spawning;
                            game.Meta.Events.Send(
                                new WaveStarted(script.Id, script.DisplayName));
                            updateResources();
                            updateSpawnQueue();
                        }
                        break;
                    case Phase.Spawning:
                        updateResources();
                        updateSpawnQueue();
                        if (game.Time >= script.SpawnEnd)
                        {
                            State.Satisfies(spawnQueue.Count == 0);
                            phase = Phase.FinishOff;
                        }
                        break;
                    case Phase.FinishOff:
                        if (spawnedUnits.Count == 0)
                        {
                            game.Meta.Events.Send(new WaveEnded(script.Id, script.TargetFaction));
                            game.Meta.Events.Unsubscribe(this);
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

            private void updateResources()
            {
                var spawnTimeElapsed = game.Time - script.SpawnStart;
                var percentageTimeElapsed = Math.Clamp(spawnTimeElapsed / script.SpawnDuration, 0, 1);
                var expectedResourcesGiven = script.ResourcesAwardedBySpawnPhase.Percentage(percentageTimeElapsed);
                State.Satisfies(expectedResourcesGiven >= resourcesGiven);
                script.TargetFaction.Resources.ProvideResources(expectedResourcesGiven - resourcesGiven);
                resourcesGiven = expectedResourcesGiven;
            }

            private void updateSpawnQueue()
            {
                while (spawnQueue.TryPeek(out var spawn) && game.Time >= spawn.Time)
                {
                    spawnQueue.Dequeue();
                    var unit = new EnemyUnit(spawn.UnitId, spawn.UnitBlueprint, spawn.SpawnLocation.Tile);
                    spawnedUnits.Add(unit);
                    game.Add(unit);
                }
            }

            public void HandleEvent(EnemyKilled @event)
            {
                spawnedUnits.Remove(@event.Unit);
            }
        }

        private sealed record EnemySpawn
        {
            public Id<EnemyUnit> UnitId { get; }
            public IUnitBlueprint UnitBlueprint { get; }
            public SpawnLocation SpawnLocation { get; }
            public Instant Time { get; }

            public EnemySpawn(
                Id<EnemyUnit> unitId, IUnitBlueprint unitBlueprint, SpawnLocation spawnLocation, Instant time)
            {
                UnitId = unitId;
                UnitBlueprint = unitBlueprint;
                SpawnLocation = spawnLocation;
                Time = time;
            }
        }
    }
}
