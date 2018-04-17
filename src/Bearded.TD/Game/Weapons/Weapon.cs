using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Model;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Weapons
{
    [ComponentOwner]
    class Weapon : IPositionable, IFactioned
    {
        private readonly WeaponBlueprint blueprint;
        private readonly ITurret turret;
        private readonly Building ownerAsBuilding;

        private readonly ComponentCollection<Weapon> components = new ComponentCollection<Weapon>();

        private Instant nextTileInRangeRecalculationTime;
        private Instant nextPossibleShootTime;
        private List<Tile<TileInfo>> tilesInRange;
        private EnemyUnit target;

        public Position2 Position => turret.Position;
        public Faction Faction => turret.OwnerFaction;

        public Weapon(WeaponBlueprint blueprint, ITurret turret)
        {
            this.blueprint = blueprint;
            this.turret = turret;
            ownerAsBuilding = turret.Owner as Building;
            nextPossibleShootTime = turret.Owner.Game.Time;

            components.Add(this, blueprint.GetComponents());
        }

        public void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);

            if (ownerAsBuilding == null)
                return;

            if (!ownerAsBuilding.IsCompleted)
            {
                nextPossibleShootTime = ownerAsBuilding.Game.Time;
                return;
            }

            updateForCompletedBuilding();
        }

        private void updateForCompletedBuilding()
        {
            var time = ownerAsBuilding.Game.Time;
            while (nextPossibleShootTime <= time)
            {
                ensureTilesInRangeList();
                ensureTargetingState();

                if (target == null)
                {
                    while (nextPossibleShootTime <= time)
                        nextPossibleShootTime += blueprint.IdleInterval;
                    break;
                }

                shootTarget();

                nextPossibleShootTime += blueprint.ShootInterval;
            }
        }

        private void ensureTilesInRangeList()
        {
            if (nextTileInRangeRecalculationTime > ownerAsBuilding.Game.Time)
                return;

            recalculateTilesInRange();
        }

        private void recalculateTilesInRange()
        {
            var rangeSquared = blueprint.Range.Squared;

            var owner = turret.Owner;
            var game = owner.Game;
            var level = game.Level;

            tilesInRange = new LevelVisibilityChecker<TileInfo>()
                .EnumerateVisibleTiles(level, turret.Position, blueprint.Range,
                    t => !t.IsValid || !t.Info.IsPassableFor(TileInfo.PassabilityLayer.Projectile))
                .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                            (level.GetPosition(t.tile) - turret.Position).LengthSquared < rangeSquared)
                .Select(t => t.tile)
                .ToList();

            nextTileInRangeRecalculationTime = game.Time + blueprint.ReCalculateTilesInRangeInterval;
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
                ownerAsBuilding.Position,
                (target.Position - ownerAsBuilding.Position).Direction,
                20.U() / 1.S(),
                blueprint.Damage,
                ownerAsBuilding
            );

            ownerAsBuilding.Game.Add(p);
        }

        private void tryFindTarget()
        {
            target = tilesInRange
                .SelectMany(t => t.Info.Enemies)
                .FirstOrDefault();
        }

        public void Draw(GeometryManager geometries)
        {
            components.Draw(geometries);

            var owner = turret.Owner as ISelectable;
            if (owner == null)
                return;

            if (owner.SelectionState == SelectionState.Default)
                return;

            recalculateTilesInRange();

            var geo = geometries.ConsoleBackground;

            geo.Color = Color.Green * (owner.SelectionState == SelectionState.Selected ? 0.15f : 0.1f);

            var level = turret.Owner.Game.Level;

            foreach (var tile in tilesInRange)
            {
                geo.DrawCircle(level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
            }
        }
    }
}
