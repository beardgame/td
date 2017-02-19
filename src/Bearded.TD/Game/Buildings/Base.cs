using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Buildings
{
    class Base : Building
    {
        private const float incomePerSecond = 1;

        public Base(PositionedFootprint footprint) : base(Blueprint, footprint)
        { }

        protected override void OnAdded()
        {
            base.OnAdded();

            foreach (var tile in OccupiedTiles)
            {
                Game.Navigator.AddSink(tile);
            }

            Game.ListAs(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            Game.Resources.AddBeardedPoints(elapsedTime.NumericValue * incomePerSecond);
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.Blue;

            geo.DrawCircle(Position.NumericValue, 1.2f * HexagonWidth, true, 6);

            var fontGeo = geometries.ConsoleFont;
            fontGeo.Color = Color.White;
            fontGeo.Height = 1;

            fontGeo.DrawString(Position.NumericValue, Health.ToString(), .5f, .5f);
        }

        protected override void OnDamaged()
        {
            base.OnDamaged();
            if (Health <= 0)
                Game.Meta.DoGameOver();
        }

        private static readonly BuildingBlueprint Blueprint
            = new BuildingBlueprint(TileSelection.FromFootprint(Footprint.CircleSeven), 1000, null);
    }
}
