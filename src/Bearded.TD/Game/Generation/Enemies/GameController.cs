using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Generation.Enemies
{
    #region Interface

    interface IGameController
    {
        GameControllerDebugParameters DebugParameters { get; }
        void Update(UpdateEventArgs args);
    }

    class DummyGameController : IGameController
    {
        public GameControllerDebugParameters DebugParameters => GameControllerDebugParameters.Empty;

        public void Update(UpdateEventArgs args)
        {
        }
    }

    struct GameControllerDebugParameters
    {
        public static GameControllerDebugParameters Empty => new GameControllerDebugParameters();

        public double Debit { get; }
        public double PayoffFactor { get; }
        public double MinWaveCost { get; }
        public double MaxWaveCost { get; }
        public double Lag { get; }

        public GameControllerDebugParameters(
            double debit, double payoffFactor, double minWaveCost, double maxWaveCost, double lag)
        {
            Debit = debit;
            PayoffFactor = payoffFactor;
            MinWaveCost = minWaveCost;
            MaxWaveCost = maxWaveCost;
            Lag = lag;
        }
    }

    #endregion

    class GameController : IGameController
    {
        private const int numAvailableSpawnPoints = 6;

        private readonly GameInstance game;
        private readonly Random random = new Random();
        private readonly LinkedList<EnemyWave> plannedWaves = new LinkedList<EnemyWave>();

        private double debit;
        private double debitPayoffFactor = Constants.Game.EnemyGeneration.InitialDebitPayoffRate;
        private double minWaveCost = Constants.Game.EnemyGeneration.InitialMinWaveCost;
        private double maxWaveCost = Constants.Game.EnemyGeneration.InitialMaxWaveCost;

        public GameControllerDebugParameters DebugParameters => new GameControllerDebugParameters(
            debit, debitPayoffFactor, minWaveCost, maxWaveCost, Constants.Game.EnemyGeneration.TimeBeforeFirstWave.NumericValue);

        public GameController(GameInstance game)
        {
            this.game = game;
        }

        public void Update(UpdateEventArgs args)
        {
            debitPayoffFactor *= Constants.Game.EnemyGeneration.DebitPayoffGrowth.Powed(args.ElapsedTimeInS);
            minWaveCost *= Constants.Game.EnemyGeneration.WaveCostGrowth.Powed(args.ElapsedTimeInS);
            maxWaveCost *= Constants.Game.EnemyGeneration.WaveCostGrowth.Powed(args.ElapsedTimeInS);
            debit -= args.ElapsedTimeInS * debitPayoffFactor;

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

            var minTimeToSpawn = numEnemies * Constants.Game.EnemyGeneration.MinTimeBetweenEnemies;
            var maxTimeToSpawn = numEnemies * Constants.Game.EnemyGeneration.MaxTimeBetweenEnemies;

            var minSpawnPoints = Mathf.FloorToInt(minTimeToSpawn / Constants.Game.EnemyGeneration.MaxWaveDuration);
            var maxSpawnPoints = Mathf.CeilToInt(maxTimeToSpawn / Constants.Game.EnemyGeneration.MinWaveDuration);
            var numSpawnPoints = random.Next(minSpawnPoints, maxSpawnPoints + 1).Clamped(1, numAvailableSpawnPoints);

            var minDuration = TimeSpan.Max(Constants.Game.EnemyGeneration.MinWaveDuration, Constants.Game.EnemyGeneration.MinTimeBetweenEnemies / numSpawnPoints * numEnemies);
            var maxDuration = TimeSpan.Min(Constants.Game.EnemyGeneration.MaxWaveDuration, Constants.Game.EnemyGeneration.MaxTimeBetweenEnemies / numSpawnPoints * numEnemies);
            var waveDuration = random.NextDouble(minDuration.NumericValue, maxDuration.NumericValue).S();

            buildWave(numSpawnPoints, blueprint, numEnemies, numSpawnPoints * waveDuration / numEnemies);
        }

        private UnitBlueprint selectBlueprint()
        {
            var blueprints = game.Blueprints.Units.All.ToList();
            var probabilities = new double[blueprints.Count + 1];
            foreach (var (blueprint, i) in blueprints.Indexed())
            {
                probabilities[i + 1] = getBlueprintProbability(blueprint) + probabilities[i];
            }
            var t = random.NextDouble(probabilities[probabilities.Length - 1]);
            var result = Array.BinarySearch(probabilities, t);
            
            return result >= 0 ? blueprints[result] : blueprints[~result - 1];
        }

        private double getBlueprintProbability(UnitBlueprint blueprint)
        {
            return 1 / blueprint.Value.Squared();
        }

        private void buildWave(int numSpawnPoints, UnitBlueprint blueprint, int numEnemies, TimeSpan timeBetweenSpawns)
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
                        game.State.Time + Constants.Game.EnemyGeneration.TimeBeforeFirstWave,
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
            private readonly UnitBlueprint blueprint;
            private readonly Instant start;
            private readonly int size;
            private readonly TimeSpan timeBetweenSpawns;
            private readonly Tile<TileInfo> tile;

            private bool warningSent;
            private int enemiesSpawned;

            public bool IsFinished => enemiesSpawned == size;

            public EnemyWave(
                GameInstance game,
                UnitBlueprint blueprint,
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
                var showWarningAt = start - Constants.Game.EnemyGeneration.WarningTime;
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
                    game.Meta.Ids.GetNext<GameUnit>()));
                enemiesSpawned++;
            }
        }
    }
}
