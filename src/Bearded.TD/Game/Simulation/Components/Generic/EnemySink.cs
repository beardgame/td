using amulware.Graphics;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    [Component("sink")]
    class EnemySink : Component<Building>
    {
        private Maybe<Health<Building>> healthComponent;

        protected override void Initialise()
        {
            foreach (var tile in Owner.OccupiedTiles)
                Owner.Game.Navigator.AddSink(tile);

            healthComponent = Owner.GetComponents<Health<Building>>().MaybeSingle();
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers)
        {
            healthComponent.Match(health =>
            {
                drawers.InGameConsoleFont.DrawLine(
                    xyz: Owner.Position.NumericValue,
                    text: health.CurrentHealth.ToString(),
                    fontHeight: 1,
                    alignHorizontal: .5f,
                    alignVertical: .5f,
                    parameters: Color.White
                );
            });
        }
    }
}
