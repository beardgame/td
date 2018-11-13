using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.EnemyGeneration;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Generation.Enemies
{
    class EnemySpawnController
    {
        private readonly GameInstance game;
        private readonly Random random = new Random();
        private readonly LinkedList<EnemyWave> plannedWaves = new LinkedList<EnemyWave>();

        private readonly IReadOnlyList<EnemySpawnDefinition> enemies;
        private IReadOnlyList<SpawnPoint> spawnPoints;

        private double debit;
        private double debitPayoffFactor = InitialDebitPayoffRate;
        private double minWaveCost = InitialMinWaveCost;
        private double maxWaveCost = InitialMaxWaveCost;

        public EnemySpawnDebugParameters DebugParameters => new EnemySpawnDebugParameters(
            debit, debitPayoffFactor, minWaveCost, maxWaveCost, TimeBeforeFirstWave.NumericValue);

        public EnemySpawnController(GameInstance game)
        {
            this.game = game;
            enemies = EnemySpawnDefinitions.BuildSpawnDefinitions();

            game.GameStateInitialized += state =>
            {
                spawnPoints = Directions
                    .All
                    .Enumerate()
                    .Select(
                        dir => new Tile<TileInfo>(game.State.Level.Tilemap, 0, 0)
                            .Offset(dir.Step() * game.State.Level.Tilemap.Radius))
                    .Select(tile => new SpawnPoint(game, tile))
                    .ToList()
                    .AsReadOnly();
            };
        }

        public void Update(TimeSpan elapsedTime)
        {
            DebugAssert.State.Satisfies(
                spawnPoints != null, "Expected the spawn points to be initialized before first update.");

            debitPayoffFactor *= DebitPayoffGrowth.Powed(elapsedTime.NumericValue);
            minWaveCost *= WaveCostGrowth.Powed(elapsedTime.NumericValue);
            maxWaveCost *= WaveCostGrowth.Powed(elapsedTime.NumericValue);
            debit -= elapsedTime.NumericValue * debitPayoffFactor;

            if (debit <= 0)
                queueNextWave();

            var curr = plannedWaves.First;
            while (curr != null)
            {
                curr.Value.Update();
                var next = curr.Next;
                if (curr.Value.IsFinished)
                    plannedWaves.Remove(curr);
                curr = next;
            }

            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.Update();
            }
        }

        private void queueNextWave()
        {
            var blueprint = selectBlueprint();

            var minEnemies = Mathf.CeilToInt(minWaveCost / blueprint.Value);
            var maxEnemies = Mathf.FloorToInt(maxWaveCost / blueprint.Value);
            var numEnemies = maxEnemies <= minEnemies ? minEnemies : random.Next(minEnemies, maxEnemies + 1);

            var minTimeToSpawn = numEnemies * MinTimeBetweenEnemies;
            var maxTimeToSpawn = numEnemies * MaxTimeBetweenEnemies;

            var minSpawnPoints = Mathf.FloorToInt(minTimeToSpawn / MaxWaveDuration);
            var maxSpawnPoints = Mathf.CeilToInt(maxTimeToSpawn / MinWaveDuration);
            var numSpawnPoints = random.Next(minSpawnPoints, maxSpawnPoints + 1).Clamped(1, spawnPoints.Count);

            var minDuration = TimeSpan.Max(MinWaveDuration, MinTimeBetweenEnemies / numSpawnPoints * numEnemies);
            var maxDuration = TimeSpan.Min(MaxWaveDuration, MaxTimeBetweenEnemies / numSpawnPoints * numEnemies);
            var waveDuration = random.NextDouble(minDuration.NumericValue, maxDuration.NumericValue).S();

            buildWave(numSpawnPoints, blueprint, numEnemies, numSpawnPoints * waveDuration / numEnemies);
        }

        private IUnitBlueprint selectBlueprint()
        {
            var probabilities = new double[enemies.Count + 1];
            foreach (var (enemy, i) in enemies.Indexed())
            {
                probabilities[i + 1] = enemy.GetProbability(game) + probabilities[i];
            }
            var t = random.NextDouble(probabilities[probabilities.Length - 1]);
            var result = Array.BinarySearch(probabilities, t);

            var definition = result >= 0 ? enemies[result] : enemies[~result - 1];
            return game.Blueprints.Units[definition.BlueprintId];
        }

        private void buildWave(int numSpawnPoints, IUnitBlueprint blueprint, int numEnemies, TimeSpan timeBetweenSpawns)
        {
            foreach (var (spawnPoint, i) in spawnPoints.RandomSubset(numSpawnPoints).Indexed())
            {
                var numEnemiesForPoint = numEnemies / numSpawnPoints;
                if (i < numEnemiesForPoint % numSpawnPoints)
                    numEnemiesForPoint++;
                plannedWaves.AddLast(
                    new EnemyWave(
                        game,
                        blueprint,
                        game.State.Time + TimeBeforeFirstWave,
                        numEnemiesForPoint,
                        timeBetweenSpawns,
                        spawnPoint));
            }

            debit = blueprint.Value * numEnemies;
        }

        private class SpawnPoint
        {
            private readonly GameInstance game;
            private readonly LinkedList<EnemyWave> plannedWaves = new LinkedList<EnemyWave>();

            public Tile<TileInfo> Tile { get; }

            private Id<UnitWarning>? warningId;

            public SpawnPoint(GameInstance game, Tile<TileInfo> tile)
            {
                this.game = game;
                Tile = tile;
            }

            public void Update()
            {
                while (plannedWaves.Count > 0 && plannedWaves.First.Value.IsFinished)
                {
                    plannedWaves.RemoveFirst();
                }

                if (plannedWaves.Count == 0 && warningId.HasValue)
                {
                    game.Meta.Dispatcher.RunOnlyOnServer(() => HideUnitSpawnWarning.Command(game, warningId.Value));
                    warningId = null;
                }
            }

            public void ShowWarningForWave(EnemyWave wave)
            {
                insertWave(wave);
                if (!warningId.HasValue)
                {
                    warningId = game.Meta.Ids.GetNext<UnitWarning>();
                    game.Meta.Dispatcher.RunOnlyOnServer(
                        () => ShowUnitSpawnWarning.Command(game, warningId.Value, Tile));
                }
            }

            private void insertWave(EnemyWave wave)
            {
                if (plannedWaves.Count == 0 || wave.EndTime >= plannedWaves.Last.Value.EndTime)
                {
                    plannedWaves.AddLast(wave);
                }
                else
                {
                    var before = plannedWaves.Last;
                    while (before.Previous != null && wave.EndTime < before.Previous.Value.EndTime)
                    {
                        before = before.Previous;
                    }

                    if (before == null)
                    {
                        plannedWaves.AddFirst(wave);
                    }
                    else
                    {
                        plannedWaves.AddBefore(before, wave);
                    }
                }
            }
        }

        private class EnemyWave
        {
            private readonly GameInstance game;
            private readonly IUnitBlueprint blueprint;
            private readonly Instant start;
            private readonly int size;
            private readonly TimeSpan timeBetweenSpawns;
            private readonly SpawnPoint spawnPoint;

            private bool warningSent;
            private int enemiesSpawned;

            public bool IsFinished => enemiesSpawned == size;
            public Instant EndTime => start + (size - 1) * timeBetweenSpawns;

            public EnemyWave(
                GameInstance game,
                IUnitBlueprint blueprint,
                Instant start,
                int size,
                TimeSpan timeBetweenSpawns,
                SpawnPoint spawnPoint)
            {
                this.game = game;
                this.blueprint = blueprint;
                this.start = start;
                this.size = size;
                this.timeBetweenSpawns = timeBetweenSpawns;
                this.spawnPoint = spawnPoint;
            }

            public void Update()
            {
                if (IsFinished)
                    return;
                if (!warningSent)
                    updateWarning();
                updateEnemySpawn();
            }

            private void updateWarning()
            {
                var showWarningAt = start - WarningTime;
                if (showWarningAt <= game.State.Time)
                    showWarning();
            }

            private void showWarning()
            {
                spawnPoint.ShowWarningForWave(this);
                warningSent = true;
            }

            private void updateEnemySpawn()
            {
                var nextSpawnAt = start + enemiesSpawned * timeBetweenSpawns;
                while (nextSpawnAt <= game.State.Time && !IsFinished)
                {
                    spawnEnemy();
                    nextSpawnAt += timeBetweenSpawns;
                }
            }

            private void spawnEnemy()
            {
                game.State.Meta.Dispatcher.RunOnlyOnServer(() => SpawnUnit.Command(
                    game.State,
                    spawnPoint.Tile,
                    blueprint,
                    game.Meta.Ids.GetNext<EnemyUnit>()));
                enemiesSpawned++;
            }
        }
    }
}
