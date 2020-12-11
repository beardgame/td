using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Graphical
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
            var position = Owner.Position.NumericValue;

            position.Z += Parameters.Height.NumericValue;

            geometries.PointLight.Draw(
                position,
                Parameters.Radius.NumericValue,
                Parameters.Color
                );
        }
    }
}
