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
        private const int numAvailableSpawnPoints = 6;

        private readonly GameInstance game;
        private readonly Random random = new Random();
        private readonly LinkedList<EnemyWave> plannedWaves = new LinkedList<EnemyWave>();

        private readonly IReadOnlyList<EnemySpawnDefinition> enemies;

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
        }

        public void Update(TimeSpan elapsedTime)
        {
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
            var numSpawnPoints = random.Next(minSpawnPoints, maxSpawnPoints + 1).Clamped(1, numAvailableSpawnPoints);

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
            foreach (var (tile, i) in selectSpawnLocations(numSpawnPoints).Indexed())
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
                        tile));
            }

            debit = blueprint.Value * numEnemies;
        }

        private IEnumerable<Tile<TileInfo>> selectSpawnLocations(int num)
        {
            return Directions
                    .All
                    .Enumerate()
                    .RandomSubset(num)
                    .Select(
                        dir => new Tile<TileInfo>(game.State.Level.Tilemap, 0, 0)
                                .Offset(dir.Step() * game.State.Level.Tilemap.Radius));
        }

        private class EnemyWave
        {
            private readonly GameInstance game;
            private readonly IUnitBlueprint blueprint;
            private readonly Instant start;
            private readonly int size;
            private readonly TimeSpan timeBetweenSpawns;
            private readonly Tile<TileInfo> tile;

            private bool warningSent;
            private int enemiesSpawned;

            public bool IsFinished => enemiesSpawned == size;

            public EnemyWave(
                GameInstance game,
                IUnitBlueprint blueprint,
                Instant start,
                int size,
                TimeSpan timeBetweenSpawns,
                Tile<TileInfo> tile)
            {
                this.game = game;
                this.blueprint = blueprint;
                this.start = start;
                this.size = size;
                this.timeBetweenSpawns = timeBetweenSpawns;
                this.tile = tile;
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
                game.State.Meta.Dispatcher.RunOnlyOnServer(() => ShowUnitSpawnWarning.Command(
                    game, tile, start + (size - 1) * timeBetweenSpawns));
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
                    tile,
                    blueprint,
                    game.Meta.Ids.GetNext<EnemyUnit>()));
                enemiesSpawned++;
            }
        }
    }
}
