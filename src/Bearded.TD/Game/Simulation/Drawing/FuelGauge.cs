using System;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("fuelGauge")]
sealed class FuelGauge : Component
{
    private IFuelTank? tank;
    private float displayLevel;
    private IBuildingStateProvider? building;
    private IStatusTracker? statusDisplay;
    private IStatusReceipt? status;

    private bool isVisible => building?.State.IsCompleted != false;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFuelTank>(Owner, Events, t => tank = t);
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, b => building = b);
        ComponentDependencies.Depend<IStatusTracker>(Owner, Events, d => statusDisplay = d);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime)
    {
        if (!isVisible || tank is not { FilledPercentage: var level })
        {
            return;
        }

        if (statusDisplay is not null && status is null)
        {
            var drawSpec =
                StatusDrawSpec.StaticIconWithProgress("fuel-tank".ToStatusIconSpriteId(), () => displayLevel);
            var drawer = new StatusDrawer(() => displayLevel);
            status =
                statusDisplay.AddStatus(new StatusSpec(StatusType.Neutral, drawSpec, null, drawer), null);
        }

        displayLevel += (level - displayLevel) * (1 - MathF.Pow(0.1f, (float)elapsedTime.NumericValue));
    }

    private sealed class StatusDrawer : IStatusDrawer
    {
        private const int fullCircleQuads = 32;
        private static readonly Direction2 minAngle = Direction2.FromDegrees(-90 - 15);
        private static readonly Direction2 maxAngle = Direction2.FromDegrees(-90 + 15);
        private static readonly Angle fullAngle = Angle.BetweenNegative(minAngle, maxAngle);

        private readonly Func<float> getDisplayLevel;

        public StatusDrawer(Func<float> getDisplayLevel)
        {
            this.getDisplayLevel = getDisplayLevel;
        }

        public void Draw(CoreDrawers core, IComponentDrawer drawer, Vector3 position, float size)
        {
            var primitives = core.CustomPrimitives;
            var constants = new FuelGaugeConstants(size);

            drawSegment(primitives, position, constants, 0f, 0.2f, Color.Red);
            drawSegment(primitives, position, constants, 0.2f, 0.4f, Color.Orange);
            drawSegment(primitives, position, constants, 0.4f, 1f, Color.Green);

            drawIndicator(core.Primitives, position, constants);
        }

        private void drawIndicator(IShapeDrawer2<Color> drawer, Vector3 center, FuelGaugeConstants constants)
        {
            var color = Color.Black;
            var angle = minAngle + fullAngle * getDisplayLevel();

            drawer.FillCircle(center, constants.InnerRadius * 0.3f, color, 16);
            drawer.DrawLine(
                center, center + angle.Vector.WithZ() * constants.IndicatorRadius, constants.IndicatorWidth, color);
        }

        private static void drawSegment(
            IDrawableSprite<Color> drawer,
            Vector3 center,
            FuelGaugeConstants constants,
            float from,
            float to,
            Color color)
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
                    topLeft: center + (vector * constants.OuterRadius).WithZ(),
                    topRight: center + (nextVector * constants.OuterRadius).WithZ(),
                    bottomLeft: center + (vector * constants.InnerRadius).WithZ(),
                    bottomRight: center + (nextVector * constants.InnerRadius).WithZ(),
                    data: color
                );

                angle = nextAngle;
                vector = nextVector;
            }
        }
    }

    private readonly record struct FuelGaugeConstants(float Size)
    {
        public float InnerRadius => 0.3f * Size;
        public float OuterRadius => 0.45f * Size;
        public float IndicatorRadius => 0.38f * Size;
        public float IndicatorWidth => 0.08f * Size;
    }
}
