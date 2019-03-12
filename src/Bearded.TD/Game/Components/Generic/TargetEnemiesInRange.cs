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
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("targetEnemiesInRange")]
    class TargetEnemiesInRange : Component<Weapon, ITargetEnemiesInRange>
    {
        private PassabilityLayer passabilityLayer;
        private TileRangeDrawer tileRangeDrawer;
    
        // mutable state
        private Instant endOfIdleTime;
        private Instant nextTileInRangeRecalculationTime;
        private Unit currentRange;
        private List<Tile> tilesInRange;
        private EnemyUnit target;

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
                tileRangeDrawer = new TileRangeDrawer(game, owner, () =>
                {
                    recalculateTilesInRange();
                    return tilesInRange;
                });
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
            // TODO: this is ugly but necessary - see comment in Weapon about component order
            Owner.ShootIn(direction);
        }

        private void goIdle()
        {
            endOfIdleTime = game.Time + Parameters.NoTargetIdleInterval;
        }
        
        private void ensureTilesInRangeList()
        {
            if (currentRange == Parameters.Range && nextTileInRangeRecalculationTime > game.Time)
                return;

            recalculateTilesInRange();
        }

        private void recalculateTilesInRange()
        {
            currentRange = Parameters.Range;
            var rangeSquared = currentRange.Squared;

            var level = game.Level;

            tilesInRange = new LevelVisibilityChecker()
                .EnumerateVisibleTiles(
                    level,
                    position,
                    currentRange,
                    t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
                .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                            (level.GetPosition(t.tile) - position).LengthSquared < rangeSquared)
                .Select(t => t.tile)
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
            tileRangeDrawer?.Draw(geometries);
        }
    }
}
