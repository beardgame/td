using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Weapons
{
    [ComponentOwner]
    class Weapon : IPositionable, IFactioned
    {
        private readonly IWeaponBlueprint blueprint;
        private readonly ITurret turret;
        private readonly Building ownerAsBuilding;
        private readonly PassabilityLayer passabilityLayer;

        private readonly ComponentCollection<Weapon> components = new ComponentCollection<Weapon>();

        private Instant nextTileInRangeRecalculationTime;
        private List<Tile> tilesInRange;
        private EnemyUnit target;
        private Instant endOfIdleTime;

        public Direction2 AimDirection { get; private set; }
        public bool ShootingThisFrame { get; private set; }

        public GameObject Owner => turret.Owner;
        public Position2 Position => turret.Position;
        public Faction Faction => turret.OwnerFaction;

        public Weapon(IWeaponBlueprint blueprint, ITurret turret)
        {
            this.blueprint = blueprint;
            this.turret = turret;
            ownerAsBuilding = turret.Owner as Building;
            passabilityLayer = turret.Owner.Game.PassabilityManager.GetLayer(Passability.Projectile);

            components.Add(this, blueprint.GetComponents());
        }

        public bool CanApplyUpgradeEffect(IUpgradeEffect upgradeEffect)
        {
            return upgradeEffect.CanApplyTo(components);
        }

        public void ApplyUpgradeEffect(IUpgradeEffect upgradeEffect)
        {
            upgradeEffect.ApplyTo(components);
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (ownerAsBuilding == null || !ownerAsBuilding.IsCompleted)
                return;

            tryShootingAtTarget();

            components.Update(elapsedTime);
        }

        private void tryShootingAtTarget()
        {
            ShootingThisFrame = false;
            
            if (endOfIdleTime > Owner.Game.Time)
                return;

            ensureTilesInRangeList();
            ensureTargetingState();

            if (target == null)
            {
                goIdle();
                return;
            }

            AimDirection = (target.Position - Position).Direction;
            ShootingThisFrame = true;
        }

        private void goIdle()
        {
            endOfIdleTime = Owner.Game.Time + blueprint.NoTargetIdleInterval;
        }

        private void ensureTilesInRangeList()
        {
            if (nextTileInRangeRecalculationTime > Owner.Game.Time)
                return;

            recalculateTilesInRange();
        }

        private void recalculateTilesInRange()
        {
            var rangeSquared = blueprint.Range.Squared;

            var owner = turret.Owner;
            var game = owner.Game;
            var level = game.Level;

            tilesInRange = new LevelVisibilityChecker()
                .EnumerateVisibleTiles(
                    level,
                    turret.Position,
                    blueprint.Range,
                    t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
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

        private void tryFindTarget()
        {
            target = tilesInRange
                .SelectMany(Owner.Game.UnitLayer.GetUnitsOnTile)
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
