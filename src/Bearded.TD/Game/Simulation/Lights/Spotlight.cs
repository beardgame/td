using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Lights
{
    [Component("spotlight")]
    class Spotlight<T> : Component<T, ISpotlightParameters>
        where T : IPositionable, IDirected
    {
        public Spotlight(ISpotlightParameters parameters) : base(parameters)
        {
        }

        protected override void Initialize()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            drawers.Spotlight.Draw(
                Owner.Position.NumericValue,
                Owner.Direction.Vector.WithZ(-0.5f / Parameters.Radius.NumericValue).Normalized(),
                Parameters.Radius.NumericValue,
                Parameters.Angle.Radians,
                Parameters.Color
                );
        }
    }
}
