using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Model;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("turret")]
    class Turret : Component<Building, TurretParameters>
    {
        private Instant nextTileInRangeRecalculationTime;
        private Instant nextPossibleShootTime;
        private List<Tile<TileInfo>> tilesInRange;
        private EnemyUnit target;

        private Position2 laserTargetPoint;
        private Instant laserTargetEndTime;

        public Turret(TurretParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            var level = Owner.Game.Level;
            var position = Owner.Position;
            var tile = level.GetTile(position);

            var rangeRadius = (int)(Parameters.Range.NumericValue / Constants.Game.World.HexagonWidth) + 1;
            var rangeSquared = Parameters.Range.Squared;

            nextPossibleShootTime = Owner.Game.Time;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var time = Owner.Game.Time;
            while (nextPossibleShootTime <= time)
            {
                ensureTilesInRangeList();
                ensureTargetingState();

                if (target == null)
                {
                    while (nextPossibleShootTime <= time)
                        nextPossibleShootTime += Parameters.IdleInterval;
                    break;
                }

                shootTarget();

                nextPossibleShootTime += Parameters.ShootInterval;
            }
        }

        private void ensureTilesInRangeList()
        {
            if (nextTileInRangeRecalculationTime > Owner.Game.Time)
                return;

            var rangeSquared = Parameters.Range.Squared;

            tilesInRange = new LevelVisibilityChecker<TileInfo>()
                .EnumerateVisibleTiles(Owner.Game.Level, Owner.Position, Parameters.Range,
                                       t => !t.IsValid || t.Info.TileType == TileInfo.Type.Wall)
                .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                       (Owner.Game.Level.GetPosition(t.tile) - Owner.Position).LengthSquared < rangeSquared)
                .Select(t => t.tile)
                .ToList();

            nextTileInRangeRecalculationTime = Owner.Game.Time + Parameters.ReCalculateTilesInRangeInterval;
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
                Owner.Position,
                (target.Position - Owner.Position).Direction,
                20.U() / 1.S(),
                Parameters.Damage,
                Owner
                );
            
            Owner.Game.Add(p);

            laserTargetPoint = target.Position;
            laserTargetEndTime = Owner.Game.Time + new TimeSpan(0.1);
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