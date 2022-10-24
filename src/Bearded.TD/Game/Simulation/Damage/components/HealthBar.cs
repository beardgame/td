using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class HealthBar : Component, IListener<DrawComponents>
{
    private IHealth health = null!;
    private IBuildingStateProvider? building;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IHealth>(Owner, Events, h => health = h);
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, b => building = b);
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
        if (building?.State.IsCompleted == false)
            return;

        var p = (float) health.HealthPercentage;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        // given the implementation of current/total, this is guaranteed by IEEE754
        if (p == 1)
            return;

        var healthColor = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);

        var topLeft = Owner.Position.NumericValue - new Vector3(.5f, .5f, 0);
        var size = new Vector2(1, .1f);

        var d = e.Core.ConsoleBackground;

        d.FillRectangle(topLeft, size, Color.DarkGray);
        d.FillRectangle(topLeft, new Vector2(size.X * p, size.Y), healthColor);
    }
}
