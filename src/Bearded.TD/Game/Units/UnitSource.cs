using System;
using System.Collections.Generic;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using Color = amulware.Graphics.Color;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Units
{
    class UnitSource : GameObject
    {
        private static readonly TimeSpan queueingDelay = new TimeSpan(10);
        private static readonly TimeSpan timeBetweenSpawns = new TimeSpan(1);

        private readonly Tile<TileInfo> tile;

        private readonly LinkedList<Tuple<Instant, UnitBlueprint>> spawnQueue = new LinkedList<Tuple<Instant, UnitBlueprint>>();

        public bool HasEnemiesQueued => spawnQueue.Count > 0;

        public UnitSource(Tile<TileInfo> tile)
        {
            this.tile = tile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.ListAs(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            while (spawnQueue.Count > 0 && spawnQueue.First.Value.Item1 <= Game.Time)
            {
                spawnEnemy(spawnQueue.First.Value.Item2);
                spawnQueue.RemoveFirst();
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!HasEnemiesQueued || Mathf.FloorToInt(Game.Time.NumericValue * 2) % 2 == 0) return;

            var geo = geometries.ConsoleBackground;
            geo.Color = Color.DarkRed * 0.5f;

            const float w = HexagonDistanceX * 0.5f - 0.1f;
            const float h = HexagonDistanceY * 0.5f - 0.1f;

            var p = Game.Level.GetPosition(tile).NumericValue;

            geo.DrawRectangle(p.X - w, p.Y - h, w * 2, h * 2);
        }

        public void QueueEnemies(UnitBlueprint blueprint, int num)
        {
            var spawnTime = Game.Time + queueingDelay;
            if (spawnQueue.Count > 0 && spawnQueue.Last.Value.Item1 + timeBetweenSpawns > spawnTime)
                spawnTime = spawnQueue.Last.Value.Item1 + timeBetweenSpawns;

            for (var i = 0; i < num; i++)
            {
                spawnQueue.AddLast(new Tuple<Instant, UnitBlueprint>(spawnTime, blueprint));
                spawnTime += timeBetweenSpawns;
            }
        }

        private void spawnEnemy(UnitBlueprint blueprint)
        {
            this.Sync(SpawnUnit.Command, tile, blueprint, Game.Meta.Ids.GetNext<GameUnit>());
        }
    }
}
