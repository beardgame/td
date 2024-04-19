using System;
using System.Drawing;
using Bearded.TD.Meta;

namespace Bearded.TD.UI.Layers;

class ViewportClippingLayerControl(string debugName) : OnTopCompositeControl(debugName)
{
    public override RenderOptions RenderOptions => new RenderOptions(getViewportFromFrame());

    private Rectangle getViewportFromFrame()
    {
        var frame = Frame;
        var topLeft = frame.TopLeft;
        var size = frame.Size;

        var scale = UserSettings.Instance.UI.UIScale;

        var (x, y, w, h) = (
            (int)(topLeft.X * scale),
            (int)(topLeft.Y * scale),
            (int)Math.Ceiling(size.X * scale),
            (int)Math.Ceiling(size.Y * scale)
        );

        var openGly = ViewportSize.Height - (y + h);

        return new Rectangle(x, openGly, w, h);
    }
}
