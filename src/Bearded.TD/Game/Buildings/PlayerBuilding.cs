using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.Game.Buildings
{
    class PlayerBuilding : Building
    {
        public PlayerBuilding(Id<Building> id, BuildingBlueprint blueprint, PositionedFootprint footprint, Faction faction)
            : base(id, blueprint, footprint, faction)
        { }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.GrayScale(60);
            if (BuildManager != null)
            {
                geo.Color = geo.Color.WithAlpha((float)BuildManager.CurrentProgressFraction * .5f + .5f).Premultiplied;
            }
            geo.DrawRectangle(Position.NumericValue - Vector2.One * .3f, Vector2.One * .6f);

            base.Draw(geometries);
        }
    }
}
