using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons
{
    [Component("targetEnemiesInRange")]
    sealed class TargetEnemiesInRange : Component<Weapon, ITargetEnemiesInRange>, IWeaponRangeDrawer, IEnemyUnitTargeter
    {
        private PassabilityLayer passabilityLayer = null!;
        private TileRangeDrawer tileRangeDrawer = null!;

        // mutable state
        private Instant endOfIdleTime;
        private Instant nextTileInRangeRecalculationTime;

        private Maybe<Angle> currentMaxTurningAngle;
        private Unit currentRange;
        private ImmutableArray<Tile> tilesInRange = ImmutableArray<Tile>.Empty;

        public EnemyUnit? Target { get; private set; }

        private bool dontDrawThisFrame;

        private GameState game => Owner.Owner.Game;
        private Position3 position => Owner.Position;

        public TargetEnemiesInRange(ITargetEnemiesInRange parameters) : base(parameters)
        {
        }

        protected override void Initialize()
        {
            passabilityLayer = game.PassabilityManager.GetLayer(Passability.Projectile);
            tileRangeDrawer = new TileRangeDrawer(
                game, () => Owner.RangeDrawStyle, getTilesToDraw, Color.Green);
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
                .ToImmutableArray();

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

        public override void Draw(CoreDrawers drawers)
        {
            tileRangeDrawer.Draw();
        }

        private IEnumerable<Tile> getTilesToDraw()
        {
            if (dontDrawThisFrame)
            {
                dontDrawThisFrame = false;
                return ImmutableHashSet<Tile>.Empty;
            }

            if (Owner.Owner is IComponentOwner componentOwner)
            {
                var allTiles = ImmutableHashSet.CreateRange(componentOwner.GetComponents<ITurret>()
                    .Select(t => (IComponentOwner) t.Weapon)
                    .SelectMany(w => w.GetComponents<IWeaponRangeDrawer>())
                    .SelectMany(ranger => ranger.TakeOverDrawingThisFrame()));
                dontDrawThisFrame = false;
                return allTiles;
            }

            recalculateTilesInRange();
            return ImmutableHashSet.CreateRange(tilesInRange);
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
