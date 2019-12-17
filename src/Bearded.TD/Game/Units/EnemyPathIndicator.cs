using amulware.Graphics;
using Bearded.TD.Game.Components.Graphical;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Units
{
    sealed class EnemyPathIndicator : GameObject, ITileWalkerOwner
    {
        private const float renderSize = .1f;
        private static readonly TimeSpan trailTimeout = 1.S();

        private readonly Tile startTile;
        private TileWalker tileWalker;
        private PassabilityLayer passabilityLayer;
        private readonly TrailTracer trail = new TrailTracer(trailTimeout, newPartDistanceThreshold: 2.U());

        public Position2 Position => tileWalker?.Position ?? Level.GetPosition(CurrentTile);
        public Tile CurrentTile => tileWalker?.CurrentTile ?? startTile;

        private Instant? deleteAt;

        public EnemyPathIndicator(Tile currentTile)
        {
            startTile = currentTile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            tileWalker = new TileWalker(this, Game.Level, startTile);

            passabilityLayer = Game.PassabilityManager.GetLayer(Passability.WalkingUnit);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (deleteAt == null)
            {
                tileWalker.Update(elapsedTime, Constants.Game.Enemy.PathIndicatorSpeed);
            }
            else if (Game.Time > deleteAt)
            {
                Delete();
            }

            var h = Game.GeometryLayer[CurrentTile].DrawInfo.Height;

            trail.Update(Game.Time, Position.WithZ(h + 0.1.U()), deleteAt != null);
        }

        public override void Draw(GeometryManager geometries)
        {
            var sprites = Game.Meta.Blueprints.Sprites["particle"];
            var sprite = sprites.Sprites.GetSprite("circle-soft");

            TrailRenderer.DrawTrail(trail, sprite, renderSize, Game.Time, trailTimeout, Color.Orange.WithAlpha(0));
        }

        public void OnTileChanged(Tile oldTile, Tile newTile) { }

        public Direction GetNextDirection()
        {
            var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
            var isPassable = passabilityLayer[CurrentTile.Neighbour(desiredDirection)].IsPassable;

            if (!isPassable)
                deleteAfterTimeout();

            return isPassable ? desiredDirection : Direction.Unknown;
        }

        private void deleteAfterTimeout()
        {
            deleteAt = Game.Time + trailTimeout;
        }
    }
}
