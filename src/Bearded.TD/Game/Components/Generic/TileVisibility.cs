using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("tileVisibility")]
    class TileVisibility<T> : Component<T, ITileVisibilityParameters>
        where T : GameObject, IPositionable
    {
        public TileVisibility(ITileVisibilityParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(GeometryManager geometries)
        {
            var level = Owner.Game.Level;

            var tiles = new LevelVisibilityChecker()
                .EnumerateVisibleTiles(level, Owner.Position, Parameters.Range,
                    tile => tile.Info.IsPassableFor(TileInfo.PassabilityLayer.Projectile));

            var radiusSquared = Parameters.Range.Squared;

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
