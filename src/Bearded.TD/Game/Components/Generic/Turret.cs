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
    class Turret<T> : Component<T, WeaponParameters>
        where T : BuildingBase<T>
    {
        private Instant nextTileInRangeRecalculationTime;
        private Instant nextPossibleShootTime;
        private List<Tile<TileInfo>> tilesInRange;
        private EnemyUnit target;
        private Building ownerAsBuilding;

        public Turret(WeaponParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            ownerAsBuilding = Owner as Building;
            nextPossibleShootTime = Owner.Game.Time;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (typeof(T) != typeof(Building))
                return;

            if (!ownerAsBuilding.IsCompleted)
            {
                nextPossibleShootTime = Owner.Game.Time;
                return;
            }

            updateForCompletedBuilding();
        }

        private void updateForCompletedBuilding()
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

            recalculateTilesInRange();
        }

        private void recalculateTilesInRange()
        {
            var rangeSquared = Parameters.Range.Squared;

            tilesInRange = new LevelVisibilityChecker<TileInfo>()
                .EnumerateVisibleTiles(Owner.Game.Level, Owner.Position, Parameters.Range,
                    t => !t.IsValid || !t.Info.IsPassableFor(TileInfo.PassabilityLayer.Projectile))
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
                ownerAsBuilding
                );
            
            Owner.Game.Add(p);
        }

        private void tryFindTarget()
        {
            target = tilesInRange
                .SelectMany(t => t.Info.Enemies)
                .FirstOrDefault();
        }

        public override void Draw(GeometryManager geometries)
        {
            if (Owner.SelectionState == SelectionState.Default)
                return;
            
            recalculateTilesInRange();

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