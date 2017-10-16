using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Model;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings.Components
{
    class Turret : Component
    {
        public static readonly Unit Range = 5.U();

        private static readonly TimeSpan ShootInterval = new TimeSpan(0.15);
        private static readonly TimeSpan IdleInterval = new TimeSpan(0.3);
        private static readonly TimeSpan ReCalculateTilesInRangeInterval = 1.S();
        private const int Damage = 10;

        private Instant nextTileInRangeRecalculationTime;
        private Instant nextPossibleShootTime;
        private List<Tile<TileInfo>> tilesInRange;
        private EnemyUnit target;

        private Position2 laserTargetPoint;
        private Instant laserTargetEndTime;

        protected override void Initialise()
        {
            var level = Building.Game.Level;
            var position = Building.Position;
            var tile = level.GetTile(position);

            var rangeRadius = (int)(Range.NumericValue / Constants.Game.World.HexagonWidth) + 1;
            var rangeSquared = Range.Squared;

            nextPossibleShootTime = Building.Game.Time;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var time = Building.Game.Time;
            while (nextPossibleShootTime <= time)
            {
                ensureTilesInRangeList();
                ensureTargetingState();

                if (target == null)
                {
                    while (nextPossibleShootTime <= time)
                        nextPossibleShootTime += IdleInterval;
                    break;
                }

                shootTarget();

                nextPossibleShootTime += ShootInterval;
            }
        }

        private void ensureTilesInRangeList()
        {
            if (nextTileInRangeRecalculationTime > Building.Game.Time)
                return;

            var rangeSquared = Range.Squared;

            tilesInRange = new LevelVisibilityChecker<TileInfo>()
                .EnumerateVisibleTiles(Building.Game.Level, Building.Position, Range,
                                       t => !t.IsValid || t.Info.TileType == TileInfo.Type.Wall)
                .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                       (Building.Game.Level.GetPosition(t.tile) - Building.Position).LengthSquared < rangeSquared)
                .Select(t => t.tile)
                .ToList();

            nextTileInRangeRecalculationTime = Building.Game.Time + ReCalculateTilesInRangeInterval;
        }

        private void ensureTargetingState()
        {
            if (target?.Deleted == true)
                target = null;

            if (target != null && !tilesInRange.Contains(target.CurrentTile))
                target = null;

            if (target != null)
                return;

            tryFindTarget();
        }

        private void shootTarget()
        {
            var p = new Projectile(
                Building.Position,
                (target.Position - Building.Position).Direction,
                20.U() / 1.S(),
                Damage, Building
                );
            
            Building.Game.Add(p);

            laserTargetPoint = target.Position;
            laserTargetEndTime = Building.Game.Time + new TimeSpan(0.1);
        }

        private void tryFindTarget()
        {
            target = tilesInRange
                .SelectMany(t => t.Info.Enemies)
                .FirstOrDefault();
        }

        public override void Draw(GeometryManager geometries)
        {
            if (Owner.SelectionState != SelectionState.Default)
            {
                var geo = geometries.ConsoleBackground;

                geo.Color = Color.Green * (Owner.SelectionState == SelectionState.Selected ? 0.15f : 0.1f);

                var level = Owner.Game.Level;

                foreach (var tile in tilesInRange)
                {
                    geo.DrawCircle(level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
                }
            }
        }
    }
}