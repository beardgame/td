using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    sealed class EnemyPathIndicator : GameObject, ITileWalkerOwner
    {
        private const float renderSize = .1f;

        private readonly Tile<TileInfo> startTile;
        private TileWalker tileWalker;
        
        public Position2 Position => tileWalker?.Position ?? Game.Level.GetPosition(CurrentTile);
        public Tile<TileInfo> CurrentTile => tileWalker?.CurrentTile ?? startTile;

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
            tileWalker.Update(elapsedTime, Constants.Game.Enemy.PathIndicatorSpeed);
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.Orange;
            geo.DrawCircle(Position.NumericValue, renderSize, true, 6);
        }

        public void OnTileChanged(Tile<TileInfo> oldTile, Tile<TileInfo> newTile) { }

        public Direction GetNextDirection()
        {
            var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
            var isPassable = CurrentTile.Neighbour(desiredDirection).Info.IsPassableFor(TileInfo.PassabilityLayer.Unit);

            if (!isPassable) Delete();

            return isPassable ? desiredDirection : Direction.Unknown;
        }
    }
}
