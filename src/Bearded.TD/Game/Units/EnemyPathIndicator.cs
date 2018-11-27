using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Components.Effects;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;
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
        private readonly Trail trail = new Trail(trailTimeout, newPartDistanceThreshold: 2.U());

        public Position2 Position => tileWalker?.Position ?? Game.Level.GetPosition(CurrentTile);
        public Tile CurrentTile => tileWalker?.CurrentTile ?? startTile;

        private Instant? deleteAt;

        public EnemyPathIndicator(Tile currentTile)
        {
            startTile = currentTile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            
            if (!Game.Level.IsValid(startTile)) throw new ArgumentOutOfRangeException();

            tileWalker = new TileWalker(this, Game.Level);
            tileWalker.Teleport(Game.Level.GetPosition(startTile), startTile);

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

            trail.Update(Game.Time, Position, deleteAt != null);
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

            var alpha = Math.Max(0, (part.Timeout - Game.Time) / trailTimeout);

            return (
                Left: (center - offset).WithZ(),
                Right: (center + offset).WithZ(),
                Color: Color.Orange.WithAlpha(0) * (float)alpha
                );
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
