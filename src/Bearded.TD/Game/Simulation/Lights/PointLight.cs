using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Lights;

[Component("pointlight")]
class PointLight : Component<PointLight.IPointLightParameters>, IListener<DrawComponents>
{
    internal interface IPointLightParameters : IParametersTemplate<IPointLightParameters>
    {
        Color Color { get; }
        Unit Radius { get; }

        Unit Height { get; }

        [Modifiable(1)]
        float Intensity { get; }

        bool DrawShadow { get; }
    }

    public PointLight(IPointLightParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
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
            Parameters.Color,
            Parameters.Intensity,
            Parameters.DrawShadow
        );
    }
}
