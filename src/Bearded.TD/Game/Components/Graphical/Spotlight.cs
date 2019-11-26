using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using Bearded.Utilities.Tilemaps.Rectangular;

namespace Bearded.TD.Game.Components.Graphical
{
    [Component("spotlight")]
    class Spotlight<T> : Component<T, ISpotlightParameters>
        where T : IPositionable, IDirected
    {
        public Spotlight(ISpotlightParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(GeometryManager geometries)
        {
            geometries.Spotlight.Draw(
                Owner.Position.NumericValue,
                Owner.Direction.Vector.WithZ(-0.5f / Parameters.Radius.NumericValue).Normalized(),
                Parameters.Radius.NumericValue,
                Parameters.Angle.Radians,
                Parameters.Color
                );
        }
    }
}
