using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Lights;

[Component("pointlight")]
class PointLight<T> : Component<T, IPointLightParameters>, IListener<DrawComponents>
    where T : IPositionable
{
    public PointLight(IPointLightParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Unsubscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var position = Owner.Position.NumericValue;

        position.Z += Parameters.Height.NumericValue;

        e.Core.PointLight.Draw(
            position,
            Parameters.Radius.NumericValue,
            Parameters.Color
        );
    }
}
