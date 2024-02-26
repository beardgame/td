using System.Collections.Generic;
using Bearded.TD.Game.Camera;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.UI.Controls.IGameWorldOverlay;

namespace Bearded.TD.UI.Controls;

interface IGameWorldOverlay
{
    public void AddControl(Control control, Vector2d size, OverlayAnchor anchor);

    public void RemoveControl(Control control);

    public readonly record struct OverlayAnchor(
        Position2 WorldPosition,
        OverlayDirection Direction,
        double Margin = Constants.UI.LayoutMargin);

    public readonly record struct OverlayDirection(double Horizontal, double Vertical, Vector2d MarginDirection)
    {
        public static readonly OverlayDirection Right = new(0, 0.5, Vector2d.UnitX);
    }
}

sealed class GameWorldOverlay(GameCamera camera)
    : OnTopCompositeControl, IGameWorldOverlay
{
    private readonly List<OverlayControl> controls = [];

    public void AddControl(Control control, Vector2d size, OverlayAnchor anchor)
    {
        var overlayControl = new OverlayControl(control, size, anchor);
        controls.Add(overlayControl);
        position(overlayControl);
        Add(control);
    }

    public void RemoveControl(Control control)
    {
        // Linear search isn't fast, but this list will never be long enough for it to really matter...
        controls.RemoveAll(c => c.Control == control);
        Remove(control);
    }

    protected override void FrameChanged()
    {
        base.FrameChanged();
        positionAll();
    }

    public override void UpdateViewport(ViewportSize viewport)
    {
        base.UpdateViewport(viewport);
        positionAll();
    }

    private void positionAll()
    {
        foreach (var c in controls)
        {
            position(c);
        }
    }

    private void position(OverlayControl control)
    {
        if (ViewportSize.Width == 0 || ViewportSize.Height == 0) return;

        var screenPos = camera.TransformWorldToScreenPos(control.Anchor.WorldPosition);
        var topLeftScreen = screenPos -
            new Vector2d(
                control.Anchor.Direction.Horizontal * control.Size.X,
                control.Anchor.Direction.Vertical * control.Size.Y
            ) +
            control.Anchor.Margin * control.Anchor.Direction.MarginDirection;
        var topLeft = topLeftScreen - Frame.TopLeft;
        control.Control.Anchor(a => a
            .Left(topLeft.X, control.Size.X)
            .Top(topLeft.Y, control.Size.Y));
    }

    private sealed record OverlayControl(Control Control, Vector2d Size, OverlayAnchor Anchor);
}