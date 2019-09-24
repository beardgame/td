using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Components.utilities;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.Maybe;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("targetEnemiesInRange")]
    class TargetEnemiesInRange : Component<Weapon, ITargetEnemiesInRange>, IWeaponRangeDrawer
    {
        private PassabilityLayer passabilityLayer;
        private TileRangeDrawer tileRangeDrawer;
    
        // mutable state
        private Instant endOfIdleTime;
        private Instant nextTileInRangeRecalculationTime;

        private Maybe<Angle> currentMaxTurningAngle;
        private Unit currentRange;
        private List<Tile> tilesInRange;
        private EnemyUnit target;

        private bool dontDrawThisFrame;

        private GameState game => Owner.Owner.Game;
        private Position2 position => Owner.Position;

        public TargetEnemiesInRange(ITargetEnemiesInRange parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
            passabilityLayer = game.PassabilityManager.GetLayer(Passability.Projectile);
            if (Owner.Owner is ISelectable owner)
            {
                tileRangeDrawer = new TileRangeDrawer(game, owner, () => getTilesToDraw());
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            tryShootingAtTarget();
        }

        private void tryShootingAtTarget()
        {
            if (endOfIdleTime > game.Time)
                return;

            ensureTilesInRangeList();
            ensureTargetingState();

            if (target == null)
            {
                goIdle();
                return;
            }

            var direction = (target.Position - position).Direction;
            Owner.AimIn(direction);
            // TODO: this is ugly but necessary - see comment in Weapon about component order
            Owner.ShootThisFrame();
        }

        private void goIdle()
        {
            endOfIdleTime = game.Time + Parameters.NoTargetIdleInterval;
        }
        
        private void ensureTilesInRangeList()
        {
            if (currentRange == Parameters.Range
                && currentMaxTurningAngle.Equals(Owner.MaximumTurningAngle)
                && nextTileInRangeRecalculationTime > game.Time)
                return;

            recalculateTilesInRange();
        }

        private void recalculateTilesInRange()
        {
            currentMaxTurningAngle = Owner.MaximumTurningAngle;
            currentRange = Parameters.Range;
            var rangeSquared = currentRange.Squared;

            var level = game.Level;
            var navigator = game.Navigator;
            
            tilesInRange = Owner.MaximumTurningAngle.Match(
                max => new LevelVisibilityChecker().InDirection(Owner.NeutralDirection, max),
                () => new LevelVisibilityChecker()
                ).EnumerateVisibleTiles(
                    level,
                    position,
                    currentRange,
                    t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
                .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                            (Level.GetPosition(t.tile) - position).LengthSquared < rangeSquared)
                .Select(t => t.tile)
                .OrderBy(navigator.GetDistanceToClosestSink)
                .ToList();

            nextTileInRangeRecalculationTime = game.Time + Parameters.ReCalculateTilesInRangeInterval;
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
                .SelectMany(game.UnitLayer.GetUnitsOnTile)
                .FirstOrDefault();
        }

        public override void Draw(GeometryManager geometries)
        {
            tileRangeDrawer?.Draw();
        }

        private Maybe<HashSet<Tile>> getTilesToDraw()
        {
            if (dontDrawThisFrame)
            {
                dontDrawThisFrame = false;
                return Nothing;
            }
            

            if (Owner.Owner is IComponentOwner componentOwner)
            {
                var allTiles = Just(new HashSet<Tile>(componentOwner.GetComponents<ITurret>()
                    .Select(t => (IComponentOwner) t.Weapon)
                    .SelectMany(w => w.GetComponents<IWeaponRangeDrawer>())
                    .SelectMany(ranger => ranger.TakeOverDrawingThisFrame())));
                dontDrawThisFrame = false;
                return allTiles;
            }

            recalculateTilesInRange();
            return Just(new HashSet<Tile>(tilesInRange));
        }

        IEnumerable<Tile> IWeaponRangeDrawer.TakeOverDrawingThisFrame()
        {
            dontDrawThisFrame = true;
            recalculateTilesInRange();
            return tilesInRange;
        }
    }

    interface IWeaponRangeDrawer
    {
        IEnumerable<Tile> TakeOverDrawingThisFrame();
    }
}
