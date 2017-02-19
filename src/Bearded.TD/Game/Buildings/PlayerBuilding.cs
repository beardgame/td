using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Buildings
{
    class PlayerBuilding : Building
    {
        public PlayerBuilding(BuildingBlueprint blueprint, PositionedFootprint footprint)
            : base(blueprint, footprint)
        { }

        public override void Update(TimeSpan elapsedTime)
        { }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.GrayScale(60);
            geo.DrawRectangle(Position.NumericValue - Vector2.One * .3f, Vector2.One * .6f);
        }
    }
}
