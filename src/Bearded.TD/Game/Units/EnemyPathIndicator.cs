using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Components.Effects;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Units
{
    sealed class EnemyPathIndicator : GameObject, ITileWalkerOwner
    {
        private const float renderSize = .1f;
        private static readonly TimeSpan trailTimeout = 1.S();

        private readonly Tile<TileInfo> startTile;
        private TileWalker tileWalker;
        private readonly Trail trail = new Trail(trailTimeout);

        public Position2 Position => tileWalker?.Position ?? Game.Level.GetPosition(CurrentTile);
        public Tile<TileInfo> CurrentTile => tileWalker?.CurrentTile ?? startTile;

        private Instant? deleteAt;

        public EnemyPathIndicator(Tile<TileInfo> currentTile)
        {
            if (!currentTile.IsValid) throw new System.ArgumentOutOfRangeException();
            startTile = currentTile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            tileWalker = new TileWalker(this, Game.Level);
            tileWalker.Teleport(Game.Level.GetPosition(startTile), startTile);
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

            trail.Update(Game.Time, Position);
        }

        public override void Draw(GeometryManager geometries)
        {
            var sprites = Game.Meta.Blueprints.Sprites["particle"];
            var sprite = sprites.Sprites.GetSprite("circle-soft");
            
            var leftUV = new Vector2(0.5f, 0);
            var rightUV = new Vector2(0.5f, 1);

            var previous = vertexLocationsFor(trail[0]);
            foreach (var part in trail.Skip(1))
            {
                var current = vertexLocationsFor(part);

                sprite.DrawQuad(
                    previous.Left, current.Left, current.Right, previous.Right,
                    leftUV, leftUV, rightUV, rightUV,
                    previous.Color, current.Color, current.Color, previous.Color
                    );

                previous = current;
            }
        }

        private (Vector3 Left, Vector3 Right, Color Color) vertexLocationsFor(Trail.Part part)
        {
            var center = part.Point.NumericValue;
            var offset = part.Normal * renderSize;

            var alpha = (part.Timeout - Game.Time) / trailTimeout;

            return (
                Left: (center - offset).WithZ(),
                Right: (center + offset).WithZ(),
                Color: Color.Orange.WithAlpha(0) * (float)alpha
                );
        }

        public void OnTileChanged(Tile<TileInfo> oldTile, Tile<TileInfo> newTile) { }

        public Direction GetNextDirection()
        {
            var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
            var isPassable = CurrentTile.Neighbour(desiredDirection).Info.IsPassableFor(TileInfo.PassabilityLayer.Unit);

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
