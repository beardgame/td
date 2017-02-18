using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Buildings
{
    class Base : Building
    {
        public Base(Tile<TileInfo> rootTile) : base(rootTile, Footprint.CircleSeven)
        { }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.ListAs(this);
        }

        public override void Update(TimeSpan elapsedTime)
        { }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.Yellow;

            geo.DrawCircle(Position.NumericValue, 1.2f * HexagonWidth, true, 6);
        }
    }
}
