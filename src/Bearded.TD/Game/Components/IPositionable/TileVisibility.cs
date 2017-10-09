using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.IPositionable
{
    class TileVisibility : Component<IPositionableGameObject>
    {
        private readonly Unit radius;

        public TileVisibility(Unit radius)
        {
            this.radius = radius;
        }

        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(GeometryManager geometries)
        {
            var level = Owner.Game.Level;

            var tiles = new LevelVisibilityChecker<TileInfo>()
                .EnumerateVisibleTiles(level, Owner.Position, radius,
                    tile => tile.Info.TileType == TileInfo.Type.Wall);

            var radiusSquared = radius.Squared;

            var geo = geometries.ConsoleBackground;

            foreach (var (tile, visibility) in tiles)
            {
                if (visibility.IsBlocking || visibility.VisiblePercentage <= 0)
                    continue;

                if ((level.GetPosition(tile) - Owner.Position).LengthSquared >= radiusSquared)
                    continue;

                geo.Color = Color.Lerp(Color.Green, Color.Orange, 1 - visibility.VisiblePercentage) * 0.25f;
                geo.DrawCircle(level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
            }
        }
    }
}
