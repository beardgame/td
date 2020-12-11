using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.Maybe;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Weapons
{
    [Component("targetEnemiesInRange")]
    class TargetEnemiesInRange : Component<Weapon, ITargetEnemiesInRange>, IWeaponRangeDrawer, IEnemyUnitTargeter
    {
        private PassabilityLayer passabilityLayer;
        private TileRangeDrawer tileRangeDrawer;

        // mutable state
        private Instant endOfIdleTime;
        private Instant nextTileInRangeRecalculationTime;

        private Maybe<Angle> currentMaxTurningAngle;
        private Unit currentRange;
        private List<Tile> tilesInRange;

        public EnemyUnit Target { get; private set; }

        private bool dontDrawThisFrame;

        private GameState game => Owner.Owner.Game;
        private Position3 position => Owner.Position;

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

            if (Target == null)
            {
                goIdle();
                return;
            }

            var direction = (Target.Position - position).XY().Direction;
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
                    position.XY(),
                    currentRange,
                    t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
                .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                            (Level.GetPosition(t.tile) - position.XY()).LengthSquared < rangeSquared)
                .Select(t => t.tile)
                .OrderBy(navigator.GetDistanceToClosestSink)
                .ToList();

            nextTileInRangeRecalculationTime = game.Time + Parameters.ReCalculateTilesInRangeInterval;
        }

        private void ensureTargetingState()
        {
            if (Target?.Deleted == true)
                Target = null;

            if (Target != null && !tilesInRange.Contains(Target.CurrentTile))
                Target = null;

            if (Target != null)
                return;

            tryFindTarget();
        }

        private void tryFindTarget()
        {
            Target = tilesInRange
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
