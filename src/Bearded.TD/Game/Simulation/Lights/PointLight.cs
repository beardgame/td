using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Lights
{
    [Component("pointlight")]
    class PointLight<T> : Component<T, IPointLightParameters>
        where T : IPositionable
    {
        public PointLight(IPointLightParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            var position = Owner.Position.NumericValue;

            position.Z += Parameters.Height.NumericValue;

            drawers.PointLight.Draw(
                position,
                Parameters.Radius.NumericValue,
                Parameters.Color
                );
        }
    }
}
