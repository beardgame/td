using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings.Components
{
    class EnemySink : Component
    {
        protected override void Initialise()
        {
            foreach (var tile in Building.OccupiedTiles)
                Building.Game.Navigator.AddSink(tile);
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries)
        {
            var fontGeo = geometries.ConsoleFont;
            fontGeo.Color = Color.White;
            fontGeo.Height = 1;

            fontGeo.DrawString(Building.Position.NumericValue, Building.Health.ToString(), .5f, .5f);
        }
    }
}
