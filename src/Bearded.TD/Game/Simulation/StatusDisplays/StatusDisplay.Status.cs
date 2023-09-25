using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.GameUI.StatusDisplay;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusDisplay
{
    private readonly List<Status> statuses = new();

    public IStatusReceipt AddStatus(StatusSpec spec, Instant? expiryTime)
    {
        var status = new Status(spec, expiryTime);
        statuses.Add(status);
        return new StatusReceipt(status, this);
    }

    private void drawStatuses(CoreDrawers core, IComponentDrawer drawer)
    {
        const float statusWidth = Width / StatusIconsPerRow;
        const float statusSize = statusWidth - ElementMargin;
        var statusOrigin = center.NumericValue +
            new Vector3(-0.5f * Width + 0.5f * statusSize,
                -ElementMargin - 0.5f * (statusWidth + PrimaryHitPointsBarHeight),
                0);

        for (var i = 0; i < statuses.Count; i++)
        {
            drawStatus(statuses[i], i);
        }

        return;

        void drawStatus(Status status, int i)
        {
            var col = i % StatusIconsPerRow;
            var row = i / StatusIconsPerRow;
            var pos = statusOrigin + statusWidth * new Vector3(col, row, 0);
            status.Drawer.Draw(core, drawer, pos, statusSize);
            // TODO: draw time until expiry
            core.Primitives.DrawRectangle(
                pos - statusSize * new Vector3(0.5f, 0.5f, 0), statusSize * Vector2.One, LineWidth, statusColor(status));
        }
    }

    private static Color statusColor(Status status) => status.Type switch
    {
        StatusType.Neutral => NeutralColor,
        StatusType.Positive => PositiveColor,
        StatusType.Negative => NegativeColor,
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    private sealed class Status
    {
        private readonly StatusSpec spec;

        public StatusType Type => spec.Type;
        public IStatusDrawer Drawer => spec.Drawer;
        public Instant? Expiry { get; set; }

        public Status(StatusSpec spec, Instant? expiryTime)
        {
            this.spec = spec;
            Expiry = expiryTime;
        }
    }

    private sealed class StatusReceipt : IStatusReceipt
    {
        private readonly Status status;
        private readonly StatusDisplay display;

        public StatusReceipt(Status status, StatusDisplay display)
        {
            this.status = status;
            this.display = display;
        }

        public void DeleteImmediately()
        {
            display.statuses.Remove(status);
        }

        public void SetExpiryTime(Instant expiryTime)
        {
            status.Expiry = expiryTime;
        }
    }
}
