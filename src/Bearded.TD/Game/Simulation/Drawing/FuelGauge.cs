using System;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("fuelGauge")]
sealed class FuelGauge : Component, IListener<DrawComponents>
{
    private IFuelTank? tank;
    private float displayLevel;
    private IBuildingStateProvider? building;

    private bool isVisible => building?.State.IsCompleted != false;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFuelTank>(Owner, Events, t => tank = t);
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, b => building = b);

        Events.Subscribe(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (!isVisible)
            return;

        if (tank is not { FilledPercentage: var level })
            return;

        displayLevel += (level - displayLevel) * (1 - MathF.Pow(0.1f, (float)elapsedTime.NumericValue));
    }

    private const float scale = 0.05f;
    private const int fullCircleQuads = 32;
    private const float innerRadius = 2 * scale;
    private const float outerRadius = 3 * scale;
    private const float indicatorRadius = 2.5f * scale;
    private const float indicatorWidth = 0.5f * scale;
    private static readonly Direction2 minAngle = Direction2.FromDegrees(-90 - 15);
    private static readonly Direction2 maxAngle = Direction2.FromDegrees(-90 + 15);
    private static readonly Angle fullAngle = Angle.BetweenNegative(minAngle, maxAngle);

    public void HandleEvent(DrawComponents e)
    {
        if (!isVisible)
            return;

        var drawer = e.Core.CustomPrimitives;

        var center = Owner.Position.NumericValue + new Vector3(-0.3f, 0.3f, 0.1f);

        drawSegment(drawer, center, 0f, 0.2f, Color.Red);
        drawSegment(drawer, center, 0.2f, 0.4f, Color.Orange);
        drawSegment(drawer, center, 0.4f, 1f, Color.Green);

        drawIndicator(e.Core.Primitives, center);
    }

    private void drawIndicator(IShapeDrawer2<Color> drawer, Vector3 center)
    {
        var color = Color.Black;
        var angle = minAngle + fullAngle * displayLevel;

        drawer.FillCircle(center, innerRadius * 0.3f, color, 16);
        drawer.DrawLine(center, center + angle.Vector.WithZ() * indicatorRadius, indicatorWidth, color);
    }

    private void drawSegment(IDrawableSprite<Color> drawer, Vector3 center, float from, float to, Color color)
    {
        var fromAngle = minAngle + fullAngle * from;
        var toAngle = minAngle + fullAngle * to;

        var quads = (int) MathF.Ceiling(fullCircleQuads * (to - from));

        var angleStep = Angle.BetweenNegative(fromAngle, toAngle) / quads;

        var angle = fromAngle;
        var vector = angle.Vector;

        for (var i = 0; i < quads; i++)
        {
            var nextAngle = angle + angleStep;
            var nextVector = nextAngle.Vector;

            drawer.DrawQuad(
                topLeft: center + (vector * outerRadius).WithZ(),
                topRight: center + (nextVector * outerRadius).WithZ(),
                bottomLeft: center + (vector * innerRadius).WithZ(),
                bottomRight: center + (nextVector * innerRadius).WithZ(),
                data: color
                );

            angle = nextAngle;
            vector = nextVector;
        }
    }
}

