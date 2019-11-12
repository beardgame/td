using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Graphical
{
    [Component("pointlight")]
    class PointLight<T> : Component<T, IPointLightParameters>
        where T : IPositionable
    {
        public PointLight(IPointLightParameters parameters) : base(parameters)
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
            geometries.PointLight.Draw(
                Owner.Position.NumericValue.WithZ(Parameters.Radius.NumericValue * 0.5f),
                Parameters.Radius.NumericValue,
                Parameters.Color
                );
        }
    }
}
