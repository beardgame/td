using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
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

            tilesInRange = new LevelVisibilityChecker<TileInfo>()
                .EnumerateVisibleTiles(Building.Game.Level, Building.Position, Range,
                                       t => !t.IsValid || t.Info.TileType == TileInfo.Type.Wall)
                .Where(((Tile<TileInfo> _, TileVisibility visibility) arg) =>
                       !arg.visibility.IsBlocking && arg.visibility.VisiblePercentage > 0.2)
                .Select(((Tile<TileInfo>, TileVisibility) arg) => arg.Item1)
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
            var position = Building.Position;
            var rangeSquared = Range.Squared;
            target = tilesInRange
                .SelectMany(t => t.Info.Enemies)
                .FirstOrDefault(e => (e.Position - position).LengthSquared < rangeSquared);
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.LineWidth = 0.05f;
            geo.Color = Color.Red * 0.5f;

            geo.DrawCircle(Building.Position.NumericValue, Range.NumericValue, false);

            var laserAlpha = (laserTargetEndTime - Building.Game.Time).NumericValue * 5;
            if (laserAlpha > 0)
            {
                geo.Color = Color.Red * (float) laserAlpha;
                geo.DrawLine(Building.Position.NumericValue, laserTargetPoint.NumericValue);
            }
        }
    }
}