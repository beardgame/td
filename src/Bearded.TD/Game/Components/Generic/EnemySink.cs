using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("sink")]
    class EnemySink : Component<Building>
    {
        private Health<Building> healthComponent;

        protected override void Initialise()
        {
            foreach (var tile in Owner.OccupiedTiles)
                Owner.Game.Navigator.AddSink(tile);

            healthComponent = Owner.GetComponent<Health<Building>>();
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries)
        {
            if (healthComponent == null)
                return;
            
            var fontGeo = geometries.ConsoleFont;
            fontGeo.Color = Color.White;
            fontGeo.Height = 1;

            fontGeo.DrawString(Owner.Position.NumericValue, healthComponent.CurrentHealth.ToString(), .5f, .5f);
        }
    }
}
