using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    class UnitWarning : GameObject
    {
        private readonly Tile<TileInfo> tile;
        private readonly Instant dieAt;

        public UnitWarning(Tile<TileInfo> tile, Instant dieAt)
        {
            this.tile = tile;
            this.dieAt = dieAt;
        }

        public override void Update(TimeSpan elapsedTime)
        {
           if (Game.Time >= dieAt)
                Delete();
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            var radius = .3f * (1.2f + Mathf.Sin((float)Game.Time.NumericValue * Mathf.Pi));
            geo.Color = Color.Red * .8f;
            geo.LineWidth = .1f;
            var position = Game.Level.GetPosition(tile);
            geo.DrawCircle(position.NumericValue, radius, false);
        }
    }
}
