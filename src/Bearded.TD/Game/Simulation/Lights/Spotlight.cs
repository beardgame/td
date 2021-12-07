using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Lights
{
    [Component("spotlight")]
    class Spotlight<T> : Component<T, ISpotlightParameters>, IDrawableComponent
        where T : IPositionable, IDirected
    {
        public Spotlight(ISpotlightParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public void Draw(IComponentDrawer drawer)
        {
            drawer.Core.Spotlight.Draw(
                Owner.Position.NumericValue,
                Owner.Direction.Vector.WithZ(-0.5f / Parameters.Radius.NumericValue).Normalized(),
                Parameters.Radius.NumericValue,
                Parameters.Angle.Radians,
                Parameters.Color
            );
        }
    }
}
