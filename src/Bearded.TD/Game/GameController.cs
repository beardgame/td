using System;
using System.Collections.Generic;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game
{
    class GameController : IGameController
    {
        private static readonly TimeSpan timeBeforeFirstWave = new TimeSpan(20);
        private const double minTimeBetweenWaves = 10;
        private const double maxTimeBetweenWaves = 30;
        private const int minWaveSize = 5;
        private const int maxWaveSize = 10;
        private const double chanceOnDoubleWave = .05;
        private static readonly TimeSpan warningTime = new TimeSpan(10);
        private static readonly TimeSpan timeBetweenEnemySpawns = TimeSpan.One;

        private readonly GameInstance game;
        private readonly Random random = new Random();
        private readonly LinkedList<EnemyWave> plannedWaves = new LinkedList<EnemyWave>();

        private Instant nextWaveStart;

        public GameController(GameInstance game)
        {
            this.game = game;
        }

        public void Update()
        {
            if (nextWaveStart == Instant.Zero)
                nextWaveStart = game.State.Time + timeBeforeFirstWave;

            if (game.State.Time >= nextWaveStart - warningTime)
            {
                queueNextWave();
            }

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
            var tile = selectSpawnLocation();
            plannedWaves.AddLast(
                new EnemyWave(
                    game,
                    nextWaveStart,
                    random.Next(minWaveSize, maxWaveSize + 1),
                    timeBetweenEnemySpawns,
                    tile));
            if (random.NextBool(chanceOnDoubleWave))
            {
                Tile<TileInfo> otherTile;
                do
                {
                    otherTile = selectSpawnLocation();
                } while (tile == otherTile);
                plannedWaves.AddLast(
                    new EnemyWave(
                        game,
                        nextWaveStart,
                        random.Next(minWaveSize, maxWaveSize + 1),
                        timeBetweenEnemySpawns,
                        otherTile));
            }
            nextWaveStart += new TimeSpan(random.NextDouble(minTimeBetweenWaves, maxTimeBetweenWaves));
        }

        private Tile<TileInfo> selectSpawnLocation()
        {
            var randomDirection =
                Directions.All.Enumerate().RandomElement(random).Step() * game.State.Level.Tilemap.Radius;
            return new Tile<TileInfo>(game.State.Level.Tilemap, 0, 0).Offset(randomDirection);
        }

        private class EnemyWave
        {
            private readonly GameInstance game;
            private readonly Instant start;
            private readonly int size;
            private readonly TimeSpan timeBetweenSpawns;
            private readonly Tile<TileInfo> tile;

            private bool warningSent;
            private int enemiesSpawned;

            public bool IsFinished => enemiesSpawned == size;

            public EnemyWave
                (GameInstance game, Instant start, int size, TimeSpan timeBetweenSpawns, Tile<TileInfo> tile)
            {
                this.game = game;
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
                    game.Blueprints.Units["debug"],
                    game.Meta.Ids.GetNext<GameUnit>()));
                enemiesSpawned++;
            }
        }
    }

    #region Interface
    interface IGameController
    {
        void Update();
    }

    class DummyGameController : IGameController
    {
        public void Update() { }
    }
    #endregion
}
