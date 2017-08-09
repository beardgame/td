using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Bearded.Utilities;

namespace Bearded.TD.Game
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

        private static readonly TimeSpan timeBeforeFirstWave = 20.S();
        private static readonly TimeSpan warningTime = 10.S();
        private static readonly TimeSpan minTimeBetweenEnemies = .1.S();
        private static readonly TimeSpan maxTimeBetweenEnemies = 2.S();
        private static readonly TimeSpan minWaveDuration = 10.S();
        private static readonly TimeSpan maxWaveDuration = 30.S();

        private const double initialMinWaveCost = 10;
        private const double initialMaxWaveCost = 14;
        private const double waveCostGrowth = 1.007;
        private const double debitPayoffGrowth = 1.009;

        private readonly GameInstance game;
        private readonly Random random = new Random();
        private readonly LinkedList<EnemyWave> plannedWaves = new LinkedList<EnemyWave>();

        private double debit;
        private double debitPayoffFactor = 1;
        private double minWaveCost = initialMinWaveCost;
        private double maxWaveCost = initialMaxWaveCost;

        public GameControllerDebugParameters DebugParameters => new GameControllerDebugParameters(
            debit, debitPayoffFactor, minWaveCost, maxWaveCost, timeBeforeFirstWave.NumericValue);

        public GameController(GameInstance game)
        {
            this.game = game;
        }

        public void Update(UpdateEventArgs args)
        {
            debitPayoffFactor *= debitPayoffGrowth.Powed(args.ElapsedTimeInS);
            minWaveCost *= waveCostGrowth.Powed(args.ElapsedTimeInS);
            maxWaveCost *= waveCostGrowth.Powed(args.ElapsedTimeInS);
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
            var blueprint = game.Blueprints.Units["debug"];

            var minEnemies = Mathf.CeilToInt(minWaveCost / blueprint.Value);
            var maxEnemies = Mathf.FloorToInt(maxWaveCost / blueprint.Value);
            var numEnemies = maxEnemies <= minEnemies ? minEnemies : random.Next(minEnemies, maxEnemies + 1);

            var minTimeToSpawn = numEnemies * minTimeBetweenEnemies;
            var maxTimeToSpawn = numEnemies * maxTimeBetweenEnemies;

            var minSpawnPoints = Mathf.FloorToInt(minTimeToSpawn / maxWaveDuration);
            var maxSpawnPoints = Mathf.CeilToInt(maxTimeToSpawn / minWaveDuration);
            var numSpawnPoints = random.Next(minSpawnPoints, maxSpawnPoints + 1).Clamped(1, numAvailableSpawnPoints);

            var minDuration = TimeSpan.Max(minWaveDuration, minTimeBetweenEnemies / numSpawnPoints * numEnemies);
            var maxDuration = TimeSpan.Min(maxWaveDuration, maxTimeBetweenEnemies / numSpawnPoints * numEnemies);
            var waveDuration = random.NextDouble(minDuration.NumericValue, maxDuration.NumericValue).S();

            buildWave(numSpawnPoints, blueprint, numEnemies, numSpawnPoints * waveDuration / numEnemies);
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
                        game.State.Time + timeBeforeFirstWave,
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
                var showWarningAt = start - warningTime;
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
