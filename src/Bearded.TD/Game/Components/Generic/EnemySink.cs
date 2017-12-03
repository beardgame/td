using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    class EnemySink : Component<Building>
    {
        protected override void Initialise()
        {
            foreach (var tile in Owner.OccupiedTiles)
                Owner.Game.Navigator.AddSink(tile);
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries)
        {
            var fontGeo = geometries.ConsoleFont;
            fontGeo.Color = Color.White;
            fontGeo.Height = 1;

            fontGeo.DrawString(Owner.Position.NumericValue, Owner.Health.ToString(), .5f, .5f);
        }
    }
}
