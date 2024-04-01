using System;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.GameUI.StatusDisplay;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusRenderer
{
    private void drawStatuses(CoreDrawers core, IComponentDrawer drawer)
    {
        const float statusWidth = Width / StatusIconsPerRow;
        const float statusSize = statusWidth - ElementMargin;
        var statusOrigin = center.NumericValue +
            new Vector3(-0.5f * Width + 0.5f * statusSize,
                -ElementMargin - 0.5f * (statusWidth + PrimaryHitPointsBarHeight),
                0);

        for (var i = 0; i < tracker.Statuses.Count; i++)
        {
            drawStatus(tracker.Statuses[i], i);
        }

        return;

        void drawStatus(Status status, int i)
        {
            var col = i % StatusIconsPerRow;
            var row = i / StatusIconsPerRow;
            var pos = statusOrigin + statusWidth * new Vector3(col, row, 0);
            status.Spec.Drawer.Draw(core, drawer, pos, statusSize);
            // TODO: draw time until expiry
            core.Primitives.DrawRectangle(
                pos - statusSize * new Vector3(0.5f, 0.5f, 0), statusSize * Vector2.One, LineWidth, statusColor(status));
        }
    }

    private static Color statusColor(Status status) => status.Spec.Type switch
    {
        StatusType.Neutral => NeutralColor,
        StatusType.Positive => PositiveColor,
        StatusType.Negative => NegativeColor,
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}
