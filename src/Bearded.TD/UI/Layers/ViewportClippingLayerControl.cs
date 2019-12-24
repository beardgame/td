using System;
using Bearded.TD.Meta;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Layers
{
    class ViewportClippingLayerControl : DefaultRenderLayerControl
    {
        public override RenderOptions RenderOptions => new RenderOptions(getViewportFromFrame());

        private ((int, int), (int, int)) getViewportFromFrame()
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

            return ((x, openGly), (w, h));
        }

        protected override void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
        {
            SkipNextRender();
        }

        protected override void RenderAsLayerAfterAncestorLayer(IRendererRouter router)
        {
            RenderAsLayer(router);
        }
    }
}
