using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("upgradePips")]
sealed class UpgradePips : Component, IListener<DrawComponents>
{
    private IBuildingUpgradeManager? buildingUpgrades;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IBuildingUpgradeManager>(Owner, Events, m => buildingUpgrades = m);

        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) {}

    private static readonly Difference3 centerOffset = new(0, 0, 0.1f);
    private const float arcRadius = 0.51f;
    private const float pipRadius = 0.05f;
    private const float pipOutlineWidth = 0.015f;
    private const int pipEdgeCount = 6;
    private static readonly Angle angleBetweenPips = Angle.FromDegrees(-13);

    private static readonly Color emptyPipColor = Color.Black;
    private static readonly Color fullPipColor = Constants.Game.GameUI.VeterancyColor;

    public void HandleEvent(DrawComponents e)
    {
        if (buildingUpgrades == null)
        {
            return;
        }

        var halfPipCount = buildingUpgrades.UpgradesInProgress.Count;
        var fullPipCount = buildingUpgrades.UpgradeSlotsOccupied - halfPipCount;
        var emptyPipCount = buildingUpgrades.UpgradeSlotsUnlocked - buildingUpgrades.UpgradeSlotsOccupied;
        var totalPipCount = emptyPipCount + halfPipCount + fullPipCount;

        if (totalPipCount == 0)
        {
            return;
        }

        var startDirection = Direction2.Zero - 0.5f * (totalPipCount - 1) * angleBetweenPips;
        var center = (Owner.Position + centerOffset).NumericValue;
        var drawer = e.Core.Primitives;

        for (var i = 0; i < totalPipCount; i++)
        {
            var pipCenter = center + arcRadius * (startDirection + i * angleBetweenPips).Vector.WithZ();
            if (i < emptyPipCount)
            {
                drawPipOutline(drawer, pipCenter, emptyPipColor);
            }
            else if (i < emptyPipCount + halfPipCount)
            {
                drawPipOutline(drawer, pipCenter, fullPipColor);
            }
            else
            {
                drawFilledPip(drawer, pipCenter, fullPipColor);
            }
        }
    }

    private static void drawPipOutline(IShapeDrawer2<Color> drawer, Vector3 center, Color color)
    {
        drawer.DrawCircle(center, pipRadius, pipOutlineWidth, color, pipEdgeCount);
    }

    private static void drawFilledPip(IShapeDrawer2<Color> drawer, Vector3 center, Color color)
    {
        drawer.FillCircle(center, pipRadius, color, pipEdgeCount);
    }
}
