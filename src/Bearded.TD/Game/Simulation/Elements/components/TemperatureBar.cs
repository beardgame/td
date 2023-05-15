using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.Elements;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("temperatureBar")]
sealed class TemperatureBar : Component, IListener<DrawComponents>
{
    private IProperty<Temperature>? temperatureProperty;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IProperty<Temperature>>(Owner, Events, property => temperatureProperty = property);
        Events.Subscribe(this);

    }

    public override void Activate() { }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(DrawComponents e)
    {
        if (temperatureProperty is not { } property)
        {
            return;
        }

        var temperature = property.Value;
        if (!shouldDraw(temperature))
        {
            return;
        }

        var p = (temperature - MinShownTemperature) / (MaxShownTemperature - MinShownTemperature) - 0.5f;
        var color = p > 0
            ? Color.FromHSVA(Color.Red.Hue, .3f + p, .8f)
            : Color.FromHSVA(Color.Cyan.Hue, .3f - p, .8f);

        var topLeft = Owner.Position.NumericValue - new Vector3(.5f, .65f, 0);
        var size = new Vector2(1, .1f);
        var topCenter = topLeft + 0.5f * size.X * Vector3.UnitX;

        var d = e.Core.ConsoleBackground;

        d.FillRectangle(topLeft, size, Color.DarkGray);
        d.FillRectangle(topCenter, new Vector2(size.X * p, size.Y), color);
    }

    private bool shouldDraw(Temperature t)
    {
        if (UserSettings.Instance.UI.AlwaysShowTemperature)
        {
            return true;
        }

        return t < MinNormalTemperature || t > MaxNormalTemperature;
    }
}
